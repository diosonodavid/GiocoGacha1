using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Post-match summary shown right after ArenaManager.ReportMatchResultAsync resolves; the
    // caller passes the result in directly (Bind) rather than this screen re-fetching it, since
    // the match just happened and the data is already in hand.
    public class ArenaResultUI : UIController
    {
        [SerializeField] private Text outcomeText;
        [SerializeField] private Text pointsDeltaText;
        [SerializeField] private Transform rewardListContainer;
        [SerializeField] private GameObject rewardEntryPrefab;

        public void Bind(bool attackerWon, ArenaMatchResultDto result)
        {
            if (outcomeText != null) outcomeText.text = attackerWon ? "Victory" : "Defeat";

            if (pointsDeltaText != null && result != null)
                pointsDeltaText.text = result.pointsDelta >= 0 ? $"+{result.pointsDelta} pts" : $"{result.pointsDelta} pts";

            BuildRewardList(result);
        }

        private void BuildRewardList(ArenaMatchResultDto result)
        {
            if (rewardListContainer == null || rewardEntryPrefab == null || result == null) return;

            for (int i = rewardListContainer.childCount - 1; i >= 0; i--)
                Destroy(rewardListContainer.GetChild(i).gameObject);

            foreach (var reward in result.rewards)
                BuildRewardEntry(reward);
        }

        private void BuildRewardEntry(StageReward reward)
        {
            var go = Instantiate(rewardEntryPrefab, rewardListContainer);
            var label = go.GetComponentInChildren<Text>();
            if (label == null) return;

            label.text = reward.item != null
                ? $"{reward.item.itemName} x{reward.amount}"
                : $"{reward.currencyType} +{reward.amount}";
        }
    }
}
