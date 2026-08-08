using System;
using System.Collections.Generic;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Core
{
    // Classic MonoBehaviour singleton (rather than an IService) so any system - combat, UI,
    // account leveling - can report progress via AchievementManager.Instance without needing to
    // wait on AppBootstrapper's ordered service list or be wired through ServiceLocator.
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        public event Action<AchievementData> OnAchievementUnlocked;
        public event Action<string, int> OnAchievementProgressChanged;

        [SerializeField] private List<AchievementData> achievements = new();

        private readonly Dictionary<string, AchievementData> achievementsById = new();
        private readonly Dictionary<string, AchievementProgress> progressById = new();
        private readonly Dictionary<AchievementStatType, List<string>> achievementIdsByStat = new();

        public IReadOnlyList<AchievementData> Achievements => achievements;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookups();
        }

        private void OnEnable()
        {
            if (ServiceLocator.Instance.TryGet<AccountLevelManager>(out var accountLevelManager))
                accountLevelManager.OnLevelUp += HandleLevelUp;
        }

        private void OnDisable()
        {
            if (ServiceLocator.Instance.TryGet<AccountLevelManager>(out var accountLevelManager))
                accountLevelManager.OnLevelUp -= HandleLevelUp;
        }

        private void BuildLookups()
        {
            achievementsById.Clear();
            achievementIdsByStat.Clear();

            foreach (var achievement in achievements)
            {
                if (achievement == null || string.IsNullOrEmpty(achievement.id)) continue;

                achievementsById[achievement.id] = achievement;
                if (!progressById.ContainsKey(achievement.id))
                    progressById[achievement.id] = new AchievementProgress { achievementId = achievement.id };

                if (!achievementIdsByStat.TryGetValue(achievement.statType, out var ids))
                    achievementIdsByStat[achievement.statType] = ids = new List<string>();
                ids.Add(achievement.id);
            }
        }

        private void HandleLevelUp(int newLevel) => ReportStat(AchievementStatType.AccountLevelReached, newLevel, additive: false);

        // Reports progress to every achievement tracking statType (e.g. cumulative damage dealt),
        // or sets the running value directly when additive is false (e.g. account level, a
        // high-water mark rather than something to sum).
        public void ReportStat(AchievementStatType statType, int amount, bool additive = true)
        {
            if (!achievementIdsByStat.TryGetValue(statType, out var ids)) return;

            foreach (var id in ids)
                ReportProgress(id, amount, additive);
        }

        public void ReportProgress(string achievementId, int amount, bool additive = true)
        {
            if (string.IsNullOrEmpty(achievementId)) return;
            if (!achievementsById.TryGetValue(achievementId, out var data)) return;
            if (!progressById.TryGetValue(achievementId, out var progress)) return;
            if (progress.isUnlocked) return;

            progress.currentValue = additive ? progress.currentValue + amount : Mathf.Max(progress.currentValue, amount);
            OnAchievementProgressChanged?.Invoke(achievementId, progress.currentValue);

            if (progress.currentValue >= data.targetValue)
            {
                progress.isUnlocked = true;
                OnAchievementUnlocked?.Invoke(data);
            }
        }

        public AchievementProgress GetProgress(string achievementId) =>
            progressById.TryGetValue(achievementId, out var progress) ? progress : null;

        public bool TryClaimReward(string achievementId)
        {
            if (!achievementsById.TryGetValue(achievementId, out var data)) return false;
            if (!progressById.TryGetValue(achievementId, out var progress)) return false;
            if (!progress.isUnlocked || progress.isClaimed) return false;

            if (data.reward.gemsReward > 0 && ServiceLocator.Instance.TryGet<CurrencyManager>(out var currencyManager))
                currencyManager.AddCurrency(CurrencyType.Gems, data.reward.gemsReward);

            if (data.reward.itemRewards.Count > 0 && ServiceLocator.Instance.TryGet<InventoryManager>(out var inventoryManager))
            {
                foreach (var item in data.reward.itemRewards)
                    inventoryManager.AddMaterial(item.itemId, 1);
            }

            progress.isClaimed = true;
            return true;
        }

        // Hydrates progress from previously saved data; additive only, so it can run right after
        // a save load without clearing progress made earlier this session.
        public void LoadState(IEnumerable<AchievementProgress> savedProgress)
        {
            if (savedProgress == null) return;

            foreach (var saved in savedProgress)
            {
                if (saved == null || string.IsNullOrEmpty(saved.achievementId)) continue;
                progressById[saved.achievementId] = saved;
            }
        }
    }
}
