using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Managers
{
    // Purchases resource packs, gacha tickets, and stamina refills, enforcing each package's
    // daily/weekly purchase limit and resetting those limits automatically once the period elapses.
    public class ShopManager : MonoBehaviour, IService
    {
        private readonly Dictionary<string, int> dailyPurchaseCounts = new();
        private readonly Dictionary<string, int> weeklyPurchaseCounts = new();

        private long lastDailyResetUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        private long lastWeeklyResetUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(ShopManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool TryPurchase(ShopPackageData package, CurrencyManager currencyManager)
        {
            if (package == null || currencyManager == null) return false;

            ResetExpiredLimits();

            var counts = GetCountsForPeriod(package.limitPeriod);
            if (counts != null && package.purchaseLimit > 0)
            {
                int currentCount = counts.TryGetValue(package.packageId, out var existing) ? existing : 0;
                if (currentCount >= package.purchaseLimit) return false;
            }

            if (!currencyManager.TrySpendCurrency(package.costCurrency, package.cost))
                return false;

            currencyManager.AddCurrency(package.rewardCurrency, package.rewardAmount);

            if (counts != null)
                counts[package.packageId] = (counts.TryGetValue(package.packageId, out var count) ? count : 0) + 1;

            return true;
        }

        public void ResetExpiredLimits()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (now - lastDailyResetUnix >= GameConstants.DailyResetIntervalSeconds)
            {
                dailyPurchaseCounts.Clear();
                lastDailyResetUnix = now;
            }

            if (now - lastWeeklyResetUnix >= GameConstants.WeeklyResetIntervalSeconds)
            {
                weeklyPurchaseCounts.Clear();
                lastWeeklyResetUnix = now;
            }
        }

        private Dictionary<string, int> GetCountsForPeriod(ShopLimitPeriod period) => period switch
        {
            ShopLimitPeriod.Daily => dailyPurchaseCounts,
            ShopLimitPeriod.Weekly => weeklyPurchaseCounts,
            _ => null
        };
    }
}
