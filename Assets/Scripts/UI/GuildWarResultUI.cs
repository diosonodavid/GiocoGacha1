using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    [Serializable]
    public class GuildWarResultEntryDto
    {
        public string guildId;
        public string guildName;
        public int finalScore;
    }

    [Serializable]
    public class GuildWarRewardEntryDto
    {
        public string rankLabel;
        public int goldReward;
        public int gemReward;
    }

    // Season-end summary: final ranking (highest score first) and the reward breakdown per rank
    // bracket. Populated by whatever screen fetches the season results, mirroring LimitedOfferUI's
    // "caller hands over the data to show" shape rather than fetching it itself.
    public class GuildWarResultUI : UIController
    {
        [SerializeField] private Text winnerNameText;
        [SerializeField] private Transform rankingContainer;
        [SerializeField] private GameObject rankingEntryPrefab;
        [SerializeField] private Transform rewardListContainer;
        [SerializeField] private GameObject rewardEntryPrefab;

        public void ShowResults(List<GuildWarResultEntryDto> ranking, List<GuildWarRewardEntryDto> rewards)
        {
            var ordered = ranking?.OrderByDescending(entry => entry.finalScore).ToList() ?? new List<GuildWarResultEntryDto>();

            if (winnerNameText != null)
                winnerNameText.text = ordered.Count > 0 ? ordered[0].guildName : string.Empty;

            RebuildRanking(ordered);
            RebuildRewards(rewards);

            Show();
        }

        private void RebuildRanking(List<GuildWarResultEntryDto> ordered)
        {
            if (rankingContainer == null || rankingEntryPrefab == null) return;

            ClearContainer(rankingContainer);
            for (int i = 0; i < ordered.Count; i++)
            {
                var go = Instantiate(rankingEntryPrefab, rankingContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"#{i + 1}  {ordered[i].guildName}  {ordered[i].finalScore:N0}";
            }
        }

        private void RebuildRewards(List<GuildWarRewardEntryDto> rewards)
        {
            if (rewardListContainer == null || rewardEntryPrefab == null || rewards == null) return;

            ClearContainer(rewardListContainer);
            foreach (var reward in rewards)
            {
                var go = Instantiate(rewardEntryPrefab, rewardListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{reward.rankLabel}: {reward.goldReward:N0} Gold, {reward.gemReward:N0} Gems";
            }
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
