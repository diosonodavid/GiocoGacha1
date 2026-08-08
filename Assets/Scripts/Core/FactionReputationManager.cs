using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Core
{
    // Mirrors AffinityManager's points-to-level pattern, but keyed per faction and with per-rank
    // currency rewards (like AchievementManager.TryClaimReward) instead of a single gating
    // threshold - each faction's rank list is walked to find the highest rank the current point
    // total qualifies for.
    public class FactionReputationManager : MonoBehaviour, IService
    {
        public event Action<string, int> OnReputationRankChanged; // factionId, newRankIndex

        [SerializeField] private List<FactionData> factions = new();

        private readonly Dictionary<string, FactionData> factionsById = new();
        private readonly Dictionary<string, int> pointsByFactionId = new();
        private readonly Dictionary<string, int> rankIndexByFactionId = new();
        private readonly Dictionary<string, HashSet<int>> claimedRanksByFactionId = new();

        public Task InitializeAsync()
        {
            factionsById.Clear();
            foreach (var faction in factions)
            {
                if (faction == null || string.IsNullOrEmpty(faction.factionId)) continue;
                factionsById[faction.factionId] = faction;
            }

            Debug.Log($"{nameof(FactionReputationManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public int GetPoints(string factionId) => pointsByFactionId.TryGetValue(factionId, out var points) ? points : 0;

        public int GetRankIndex(string factionId) => rankIndexByFactionId.TryGetValue(factionId, out var rank) ? rank : -1;

        public bool HasReachedRank(string factionId, int rankIndex) => GetRankIndex(factionId) >= rankIndex;

        public void AddReputation(string factionId, int amount)
        {
            if (amount <= 0 || !factionsById.TryGetValue(factionId, out var faction)) return;

            int newPoints = GetPoints(factionId) + amount;
            pointsByFactionId[factionId] = newPoints;

            int newRankIndex = GetRankIndex(factionId);
            for (int i = 0; i < faction.ranks.Count; i++)
            {
                if (newPoints >= faction.ranks[i].requiredReputationPoints) newRankIndex = i;
            }

            if (newRankIndex == GetRankIndex(factionId)) return;

            rankIndexByFactionId[factionId] = newRankIndex;
            OnReputationRankChanged?.Invoke(factionId, newRankIndex);
        }

        public bool TryClaimRankReward(string factionId, int rankIndex)
        {
            if (!factionsById.TryGetValue(factionId, out var faction)) return false;
            if (rankIndex < 0 || rankIndex >= faction.ranks.Count) return false;
            if (rankIndex > GetRankIndex(factionId)) return false;

            if (!claimedRanksByFactionId.TryGetValue(factionId, out var claimed))
                claimedRanksByFactionId[factionId] = claimed = new HashSet<int>();
            if (!claimed.Add(rankIndex)) return false;

            var rank = faction.ranks[rankIndex];
            if (ServiceLocator.Instance.TryGet<CurrencyManager>(out var currencyManager))
            {
                if (rank.goldReward > 0) currencyManager.AddCurrency(CurrencyType.Gold, rank.goldReward);
                if (rank.gemsReward > 0) currencyManager.AddCurrency(CurrencyType.Gems, rank.gemsReward);
            }

            return true;
        }
    }
}
