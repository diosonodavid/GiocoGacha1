using System;
using System.Threading.Tasks;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Core
{
    // Tracks the highest tower floor reached and the last time its periodic (weekly) reward was
    // claimed, resetting eligibility automatically once the period elapses - mirrors
    // ShopManager's daily/weekly limit-reset pattern.
    public class TowerManager : MonoBehaviour, IService
    {
        public event Action<int> OnMaxFloorReached;

        [SerializeField] private int maxFloorReached;

        private long lastWeeklyRewardClaimUnix;
        private bool hasClaimedWeeklyReward;

        public int MaxFloorReached => maxFloorReached;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(TowerManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void LoadState(int savedMaxFloor) => maxFloorReached = Mathf.Max(0, savedMaxFloor);

        public void ReportFloorCleared(int floorNumber)
        {
            if (floorNumber <= maxFloorReached) return;

            maxFloorReached = floorNumber;
            OnMaxFloorReached?.Invoke(maxFloorReached);
        }

        public bool CanClaimWeeklyReward()
        {
            ResetWeeklyRewardIfNeeded();
            return !hasClaimedWeeklyReward;
        }

        public bool TryClaimWeeklyReward()
        {
            if (!CanClaimWeeklyReward()) return false;

            hasClaimedWeeklyReward = true;
            return true;
        }

        private void ResetWeeklyRewardIfNeeded()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - lastWeeklyRewardClaimUnix < GameConstants.WeeklyResetIntervalSeconds) return;

            lastWeeklyRewardClaimUnix = now;
            hasClaimedWeeklyReward = false;
        }
    }
}
