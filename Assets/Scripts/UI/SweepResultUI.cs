using System.Collections.Generic;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // One-shot popup for the rewards granted by a Sweep/Skip ticket (an instant clear without
    // playing the battle); populated and shown explicitly rather than bound to a manager event,
    // since a sweep result comes back as a direct return value from whatever call triggered it.
    public class SweepResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform rewardListContainer;
        [SerializeField] private GameObject rewardEntryPrefab;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Show(IReadOnlyList<StageReward> rewards)
        {
            if (panelRoot == null) return;

            if (rewardListContainer != null && rewardEntryPrefab != null)
            {
                for (int i = rewardListContainer.childCount - 1; i >= 0; i--)
                    Destroy(rewardListContainer.GetChild(i).gameObject);

                if (rewards != null)
                {
                    foreach (var reward in rewards)
                        BuildRewardEntry(reward);
                }
            }

            panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
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
