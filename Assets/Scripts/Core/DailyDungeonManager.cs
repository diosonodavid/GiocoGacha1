using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Core
{
    // Tracks per-dungeon daily attempt counts (reset on GameConstants.DailyResetIntervalSeconds,
    // the same pattern ShopManager/TowerManager use) and filters the configured dungeon list down
    // to whichever ones are open on the current UTC day of week.
    public class DailyDungeonManager : MonoBehaviour, IService
    {
        [SerializeField] private List<DailyDungeonData> dungeons = new();
        [SerializeField] private int maxAttemptsPerDay = 3;

        private readonly Dictionary<string, int> attemptsUsedToday = new();
        private long lastDailyResetUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public IReadOnlyList<DailyDungeonData> AllDungeons => dungeons;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(DailyDungeonManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public IEnumerable<DailyDungeonData> GetDungeonsOpenToday() =>
            dungeons.Where(d => d != null && d.openDays.Contains(DateTime.UtcNow.DayOfWeek));

        public int GetAttemptsRemaining(string dungeonId)
        {
            ResetIfExpired();
            int used = attemptsUsedToday.TryGetValue(dungeonId, out var count) ? count : 0;
            return Mathf.Max(0, maxAttemptsPerDay - used);
        }

        public bool TryConsumeAttempt(string dungeonId)
        {
            if (GetAttemptsRemaining(dungeonId) <= 0) return false;
            attemptsUsedToday[dungeonId] = (attemptsUsedToday.TryGetValue(dungeonId, out var count) ? count : 0) + 1;
            return true;
        }

        private void ResetIfExpired()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now - lastDailyResetUnix < GameConstants.DailyResetIntervalSeconds) return;

            attemptsUsedToday.Clear();
            lastDailyResetUnix = now;
        }
    }
}
