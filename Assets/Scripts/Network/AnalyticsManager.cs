using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    [Serializable]
    public class AnalyticsEventPayload
    {
        public string eventName;
        public string eventDataJson;
        public long timestampUnix;
    }

    // Fire-and-forget event tracker: no client-side queue, posts each event immediately, matching
    // CrashReportService's send-on-capture approach rather than a batched flush. Convenience
    // methods serialize small [Serializable] structs (not anonymous types) through JsonUtility,
    // since JsonUtility only reads public fields and anonymous types expose properties instead.
    public class AnalyticsManager : MonoBehaviour, IService
    {
        [Serializable] private struct TutorialCompletedData { public int stepIndex; }
        [Serializable] private struct GachaPullEventData { public string bannerId; public int pullCount; }
        [Serializable] private struct BattleLostData { public string stageId; }

        private NetworkManager networkManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Debug.Log($"{nameof(AnalyticsManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void TrackEvent(string eventName, object eventData = null)
        {
            if (string.IsNullOrEmpty(eventName) || networkManager == null) return;

            var payload = new AnalyticsEventPayload
            {
                eventName = eventName,
                eventDataJson = eventData != null ? JsonUtility.ToJson(eventData) : null,
                timestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _ = SendEventAsync(payload);
        }

        public void TrackTutorialCompleted(int stepIndex) =>
            TrackEvent("tutorial_completed", new TutorialCompletedData { stepIndex = stepIndex });

        public void TrackGachaPull(string bannerId, int pullCount) =>
            TrackEvent("gacha_pull", new GachaPullEventData { bannerId = bannerId, pullCount = pullCount });

        public void TrackBattleLost(string stageId) =>
            TrackEvent("battle_lost", new BattleLostData { stageId = stageId });

        private async Task<bool> SendEventAsync(AnalyticsEventPayload payload)
        {
            var response = await networkManager.PostAsync<object>("/analytics/event", payload);
            return response.success;
        }
    }
}
