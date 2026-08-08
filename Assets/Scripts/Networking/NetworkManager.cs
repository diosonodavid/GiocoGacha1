using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GachaGame.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace GachaGame.Networking
{
    [Serializable]
    public class ApiErrorPayload
    {
        public string code;
        public string message;
    }

    [Serializable]
    public class ApiResponse<T>
    {
        public bool success;
        public T data;
        public ApiErrorPayload error;
    }

    // Talks REST (UnityWebRequest, JSON via JsonUtility) to the Express API and a raw WebSocket
    // (System.Net.WebSockets.ClientWebSocket) for realtime co-op events. Note: the backend's
    // realtime layer runs on Socket.io, whose wire protocol (Engine.IO) is not plain WebSocket
    // framing - pairing this client with it as-is only works if the server also exposes a raw
    // `ws` endpoint, otherwise a Socket.io-compatible client library is needed here instead.
    public class NetworkManager : MonoBehaviour, IService
    {
        public event Action<string> OnSocketMessageReceived;
        public event Action OnSocketConnected;
        public event Action OnSocketDisconnected;

        [SerializeField] private string apiBaseUrl = "https://localhost:3443/api";
        [SerializeField] private string socketUrl = "wss://localhost:3443/socket";

        private string accessToken;
        private ClientWebSocket webSocket;
        private CancellationTokenSource receiveLoopCts;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(NetworkManager)} initialized.");
            return Task.CompletedTask;
        }

        public async Task ShutdownAsync() => await DisconnectSocketAsync();

        public void SetAccessToken(string token) => accessToken = token;

        public Task<ApiResponse<T>> GetAsync<T>(string path)
        {
            var request = UnityWebRequest.Get(apiBaseUrl + path);
            ApplyAuthHeader(request);
            return SendRequestAsync<T>(request);
        }

        public Task<ApiResponse<T>> PostAsync<T>(string path, object body)
        {
            string json = body != null ? JsonUtility.ToJson(body) : "{}";
            var request = new UnityWebRequest(apiBaseUrl + path, UnityWebRequest.kHttpVerbPOST);
            byte[] payload = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(payload);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyAuthHeader(request);
            return SendRequestAsync<T>(request);
        }

        private void ApplyAuthHeader(UnityWebRequest request)
        {
            if (!string.IsNullOrEmpty(accessToken))
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        }

        private static async Task<ApiResponse<T>> SendRequestAsync<T>(UnityWebRequest request)
        {
            using (request)
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone) await Task.Yield();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return new ApiResponse<T>
                    {
                        success = false,
                        error = new ApiErrorPayload { code = "NETWORK_ERROR", message = request.error }
                    };
                }

                try
                {
                    return JsonUtility.FromJson<ApiResponse<T>>(request.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    return new ApiResponse<T>
                    {
                        success = false,
                        error = new ApiErrorPayload { code = "PARSE_ERROR", message = ex.Message }
                    };
                }
            }
        }

        public async Task ConnectSocketAsync()
        {
            await DisconnectSocketAsync();

            webSocket = new ClientWebSocket();
            if (!string.IsNullOrEmpty(accessToken))
                webSocket.Options.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            receiveLoopCts = new CancellationTokenSource();
            await webSocket.ConnectAsync(new Uri(socketUrl), receiveLoopCts.Token);
            OnSocketConnected?.Invoke();

            _ = ReceiveLoopAsync(receiveLoopCts.Token);
        }

        public async Task DisconnectSocketAsync()
        {
            receiveLoopCts?.Cancel();

            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client shutdown", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error closing WebSocket: {ex.Message}");
                }
            }

            webSocket?.Dispose();
            webSocket = null;
        }

        public async Task SendSocketMessageAsync(string json)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open) return;
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            var buffer = new byte[8192];
            try
            {
                while (webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    using var messageStream = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close) break;
                        messageStream.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    string message = Encoding.UTF8.GetString(messageStream.ToArray());
                    OnSocketMessageReceived?.Invoke(message);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on manual disconnect.
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"WebSocket receive loop ended: {ex.Message}");
            }
            finally
            {
                OnSocketDisconnected?.Invoke();
            }
        }
    }
}
