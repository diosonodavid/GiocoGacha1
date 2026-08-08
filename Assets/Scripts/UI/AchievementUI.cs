using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Trophy list screen: one entry per AchievementData with its current progress and a claim
    // button that's only interactable once AchievementManager reports the trophy as unlocked and
    // not yet claimed.
    public class AchievementUI : UIController
    {
        [SerializeField] private Transform achievementListContainer;
        [SerializeField] private GameObject achievementEntryPrefab;

        protected override void OnShown()
        {
            if (AchievementManager.Instance == null)
            {
                Debug.LogWarning($"{nameof(AchievementUI)} could not find an {nameof(AchievementManager)} instance.", this);
                return;
            }

            AchievementManager.Instance.OnAchievementProgressChanged += HandleProgressChanged;
            RefreshList();
        }

        protected override void OnHidden()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementProgressChanged -= HandleProgressChanged;
        }

        public void RefreshList()
        {
            if (AchievementManager.Instance == null || achievementListContainer == null || achievementEntryPrefab == null) return;

            for (int i = achievementListContainer.childCount - 1; i >= 0; i--)
                Destroy(achievementListContainer.GetChild(i).gameObject);

            foreach (var achievement in AchievementManager.Instance.Achievements)
                BuildEntry(achievement);
        }

        private void BuildEntry(AchievementData achievement)
        {
            var go = Instantiate(achievementEntryPrefab, achievementListContainer);
            var progress = AchievementManager.Instance.GetProgress(achievement.id);

            var label = go.GetComponentInChildren<Text>();
            if (label != null)
                label.text = $"{achievement.title}\n{achievement.description}\n{(progress?.currentValue ?? 0)}/{achievement.targetValue}";

            var claimButton = go.GetComponentInChildren<Button>();
            if (claimButton != null)
            {
                claimButton.interactable = progress != null && progress.isUnlocked && !progress.isClaimed;
                claimButton.onClick.AddListener(() => ClaimReward(achievement.id));
            }
        }

        public void ClaimReward(string achievementId)
        {
            if (AchievementManager.Instance == null) return;
            if (AchievementManager.Instance.TryClaimReward(achievementId))
                RefreshList();
        }

        private void HandleProgressChanged(string achievementId, int newValue) => RefreshList();
    }
}
