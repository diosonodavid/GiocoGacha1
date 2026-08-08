using System.Threading.Tasks;
using GachaGame.Core;
using UnityEngine;

namespace GachaGame.Managers
{
    public class ResourceManager : MonoBehaviour, IService
    {
        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(ResourceManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            Resources.UnloadUnusedAssets();
            return Task.CompletedTask;
        }

        public Task<T> LoadAsync<T>(string resourcePath) where T : Object
        {
            var tcs = new TaskCompletionSource<T>();
            var request = Resources.LoadAsync<T>(resourcePath);
            request.completed += _ => tcs.SetResult(request.asset as T);
            return tcs.Task;
        }

        public async Task<T> LoadJsonAsync<T>(string resourcePath)
        {
            var textAsset = await LoadAsync<TextAsset>(resourcePath);
            return textAsset != null ? JsonUtility.FromJson<T>(textAsset.text) : default;
        }
    }
}
