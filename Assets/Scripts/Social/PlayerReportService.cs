using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Social
{
    // Reports go to the backend for moderation review; blocking is purely a client-side filter
    // (no server round-trip) that ChatUI/PvPUI-style screens can consult to hide a user's messages
    // or hide them from opponent lists, mirroring GuildWarMatchmaking's "fetch candidates, filter
    // locally" restraint.
    public class PlayerReportService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;
        private readonly HashSet<string> blockedUserIds = new();

        public IReadOnlyCollection<string> BlockedUserIds => blockedUserIds;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Debug.Log($"{nameof(PlayerReportService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<bool> ReportPlayerAsync(string reportedUserId, string reasonCode, string details)
        {
            if (networkManager == null || string.IsNullOrEmpty(reportedUserId)) return false;

            var response = await networkManager.PostAsync<object>("/social/report", new ReportBody
            {
                reportedUserId = reportedUserId,
                reasonCode = reasonCode,
                details = details
            });

            return response.success;
        }

        public void BlockPlayer(string userId)
        {
            if (!string.IsNullOrEmpty(userId)) blockedUserIds.Add(userId);
        }

        public void UnblockPlayer(string userId) => blockedUserIds.Remove(userId);

        public bool IsBlocked(string userId) => !string.IsNullOrEmpty(userId) && blockedUserIds.Contains(userId);

        [Serializable]
        private class ReportBody
        {
            public string reportedUserId;
            public string reasonCode;
            public string details;
        }
    }
}
