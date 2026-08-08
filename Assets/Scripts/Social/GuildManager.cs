using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Social
{
    [Serializable]
    public class GuildChatMessageDto
    {
        public string senderId;
        public string message;
        public long timestampUnix;
    }

    // Talks to the guild REST/socket endpoints (create/join/donate/chat) through NetworkManager,
    // and caches the player's current guild plus a rolling chat history locally so the UI doesn't
    // need to poll every frame.
    public class GuildManager : MonoBehaviour, IService
    {
        public event Action<GuildData> OnGuildChanged;
        public event Action<GuildChatMessageDto> OnChatMessageReceived;

        private const int ChatHistoryLimit = 100;

        private NetworkManager networkManager;
        private readonly List<GuildChatMessageDto> chatHistory = new();
        private long lastDonationResetUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        private bool hasDonatedToday;

        public GuildData CurrentGuild { get; private set; }
        public IReadOnlyList<GuildChatMessageDto> ChatHistory => chatHistory;
        public bool HasDonatedToday => hasDonatedToday;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            if (networkManager != null) networkManager.OnSocketMessageReceived += HandleSocketMessage;

            Debug.Log($"{nameof(GuildManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            if (networkManager != null) networkManager.OnSocketMessageReceived -= HandleSocketMessage;
            return Task.CompletedTask;
        }

        public async Task<bool> CreateGuildAsync(string guildName)
        {
            if (networkManager == null) return false;

            var response = await networkManager.PostAsync<GuildData>("/guild/create", new CreateGuildBody { guildName = guildName });
            if (!response.success || response.data == null) return false;

            SetCurrentGuild(response.data);
            return true;
        }

        public async Task<bool> RequestJoinAsync(string guildId)
        {
            if (networkManager == null) return false;

            var response = await networkManager.PostAsync<ApiOkPayload>("/guild/join-request", new JoinGuildBody { guildId = guildId });
            return response.success;
        }

        public async Task<bool> TryDonateAsync()
        {
            if (networkManager == null || CurrentGuild == null) return false;

            ResetDailyDonationIfNeeded();
            if (hasDonatedToday) return false;

            var response = await networkManager.PostAsync<GuildData>("/guild/donate", new DonateGuildBody { guildId = CurrentGuild.guildId });
            if (!response.success) return false;

            hasDonatedToday = true;
            if (response.data != null) SetCurrentGuild(response.data);
            return true;
        }

        // senderId is left for the server to fill in from the authenticated session (same trust
        // model NetworkManager already uses for its Bearer token), not supplied by the client.
        public async Task SendChatMessageAsync(string message)
        {
            if (networkManager == null || CurrentGuild == null || string.IsNullOrEmpty(message)) return;

            await networkManager.SendSocketMessageAsync(JsonUtility.ToJson(new GuildChatMessageDto
            {
                message = message,
                timestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }));
        }

        private void HandleSocketMessage(string json)
        {
            GuildChatMessageDto message;
            try { message = JsonUtility.FromJson<GuildChatMessageDto>(json); }
            catch (Exception) { return; }

            if (message == null || string.IsNullOrEmpty(message.message)) return;

            chatHistory.Add(message);
            if (chatHistory.Count > ChatHistoryLimit) chatHistory.RemoveAt(0);
            OnChatMessageReceived?.Invoke(message);
        }

        private void SetCurrentGuild(GuildData guild)
        {
            CurrentGuild = guild;
            OnGuildChanged?.Invoke(guild);
        }

        private void ResetDailyDonationIfNeeded()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - lastDonationResetUnix < GameConstants.DailyResetIntervalSeconds) return;

            lastDonationResetUnix = now;
            hasDonatedToday = false;
        }

        [Serializable] private class CreateGuildBody { public string guildName; }
        [Serializable] private class JoinGuildBody { public string guildId; }
        [Serializable] private class DonateGuildBody { public string guildId; }
        [Serializable] private class ApiOkPayload { public bool ok; }
    }
}
