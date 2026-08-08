using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using GachaGame.Utilities;
using UnityEngine;

namespace GachaGame.Social
{
    // World/Private/System channels ride NetworkManager's shared raw WebSocket directly, the same
    // way GuildManager already does for guild chat (see NetworkManager's own comment about the
    // backend's Socket.io/raw-ws framing caveat, which applies here too). The Guild channel is
    // deliberately NOT re-implemented here: GuildManager already owns sending/receiving guild
    // chat traffic (SendChatMessageAsync / OnChatMessageReceived), so ChatManager delegates
    // Guild-channel sends to it and mirrors its received messages into this class's unified
    // history/event instead of parsing guild traffic a second time off the same socket stream.
    public class ChatManager : MonoBehaviour, IService
    {
        public event Action<ChatMessageData> OnMessageReceived;

        private const int MaxHistoryPerChannel = 200;

        private NetworkManager networkManager;
        private GuildManager guildManager;
        private ProfanityFilter profanityFilter;
        private readonly Dictionary<ChatChannel, List<ChatMessageData>> historyByChannel = new();

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            ServiceLocator.Instance.TryGet(out guildManager);

            if (networkManager != null) networkManager.OnSocketMessageReceived += HandleSocketMessage;
            if (guildManager != null) guildManager.OnChatMessageReceived += HandleGuildMessage;

            profanityFilter = new ProfanityFilter();
            Debug.Log($"{nameof(ChatManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            if (networkManager != null) networkManager.OnSocketMessageReceived -= HandleSocketMessage;
            if (guildManager != null) guildManager.OnChatMessageReceived -= HandleGuildMessage;
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(ChatChannel channel, string senderId, string senderName, string message, string recipientId = null)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            string censored = profanityFilter.Censor(message);

            if (channel == ChatChannel.Guild)
            {
                if (guildManager != null) await guildManager.SendChatMessageAsync(censored);
                return;
            }

            if (networkManager == null) return;

            var chatMessage = new ChatMessageData
            {
                channel = channel,
                senderId = senderId,
                senderName = senderName,
                recipientId = recipientId,
                message = censored,
                sentAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            await networkManager.SendSocketMessageAsync(JsonUtility.ToJson(chatMessage));
        }

        private void HandleSocketMessage(string json)
        {
            ChatMessageData message;
            try { message = JsonUtility.FromJson<ChatMessageData>(json); }
            catch (Exception) { return; }

            // A well-formed World/Private/System ChatMessageData always carries a non-empty
            // senderName; GuildChatMessageDto has no such field, so a guild message parsed
            // against this shape leaves it empty and is skipped here (GuildManager already
            // handled it via HandleGuildMessage below).
            if (message == null || string.IsNullOrEmpty(message.message) || string.IsNullOrEmpty(message.senderName)) return;
            if (message.channel == ChatChannel.Guild) return;

            AppendAndNotify(message);
        }

        private void HandleGuildMessage(GuildChatMessageDto guildMessage)
        {
            if (guildMessage == null) return;

            AppendAndNotify(new ChatMessageData
            {
                channel = ChatChannel.Guild,
                senderId = guildMessage.senderId,
                senderName = guildMessage.senderId,
                message = guildMessage.message,
                sentAtUnix = guildMessage.timestampUnix
            });
        }

        private void AppendAndNotify(ChatMessageData message)
        {
            if (!historyByChannel.TryGetValue(message.channel, out var list))
                historyByChannel[message.channel] = list = new List<ChatMessageData>();

            list.Add(message);
            if (list.Count > MaxHistoryPerChannel) list.RemoveAt(0);

            OnMessageReceived?.Invoke(message);
        }

        public IReadOnlyList<ChatMessageData> GetHistory(ChatChannel channel) =>
            historyByChannel.TryGetValue(channel, out var list) ? list : Array.Empty<ChatMessageData>();
    }
}
