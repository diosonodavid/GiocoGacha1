using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Core
{
    public class ActiveExpedition
    {
        public string slotId;
        public ExpeditionData data;
        public List<string> characterInstanceIds = new();
        public long startTimeUnix;
        public long endTimeUnix;
    }

    // Sends a team of characters on a timed background mission; progress is tracked purely by
    // wall-clock unix timestamps (same convention as ShopManager's reset timers) so remaining time
    // is correct even if the app was closed and reopened mid-expedition.
    public class ExpeditionManager : MonoBehaviour, IService
    {
        public event Action<string> OnExpeditionClaimed;

        private readonly Dictionary<string, ActiveExpedition> activeExpeditions = new();
        private InventoryManager inventoryManager;

        public IReadOnlyDictionary<string, ActiveExpedition> ActiveExpeditions => activeExpeditions;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            Debug.Log($"{nameof(ExpeditionManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool StartExpedition(string slotId, ExpeditionData data, List<string> characterInstanceIds)
        {
            if (string.IsNullOrEmpty(slotId) || data == null || characterInstanceIds == null) return false;
            if (activeExpeditions.ContainsKey(slotId)) return false;
            if (characterInstanceIds.Count == 0 || characterInstanceIds.Count > data.teamCapacity) return false;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            activeExpeditions[slotId] = new ActiveExpedition
            {
                slotId = slotId,
                data = data,
                characterInstanceIds = characterInstanceIds,
                startTimeUnix = now,
                endTimeUnix = now + data.durationMinutes * 60L
            };
            return true;
        }

        public TimeSpan GetRemainingTime(string slotId)
        {
            if (!activeExpeditions.TryGetValue(slotId, out var expedition)) return TimeSpan.Zero;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long remaining = Math.Max(0, expedition.endTimeUnix - now);
            return TimeSpan.FromSeconds(remaining);
        }

        public bool IsComplete(string slotId) =>
            activeExpeditions.TryGetValue(slotId, out var expedition) &&
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= expedition.endTimeUnix;

        public bool TryClaim(string slotId)
        {
            if (!IsComplete(slotId) || inventoryManager == null) return false;

            var expedition = activeExpeditions[slotId];
            foreach (var reward in expedition.data.rewardItems)
            {
                if (reward.item == null) continue;
                inventoryManager.AddMaterial(reward.item.itemId, reward.amount);
            }

            activeExpeditions.Remove(slotId);
            OnExpeditionClaimed?.Invoke(slotId);
            return true;
        }
    }
}
