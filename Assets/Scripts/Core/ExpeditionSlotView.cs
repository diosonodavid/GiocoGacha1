using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.Core
{
    // Visual widget for a single expedition slot: shows the countdown, a claim button once
    // complete, and a gem-gated quick-complete button to skip the remaining wait.
    public class ExpeditionSlotView : MonoBehaviour
    {
        private const int GemsPerMinuteSkipped = 2;

        [SerializeField] private Text timerText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button quickCompleteButton;

        private ExpeditionManager expeditionManager;
        private CurrencyManager currencyManager;
        private string slotId;

        public void Bind(string slotId)
        {
            this.slotId = slotId;
            ServiceLocator.Instance.TryGet(out expeditionManager);
            ServiceLocator.Instance.TryGet(out currencyManager);

            if (claimButton != null) claimButton.onClick.AddListener(HandleClaimPressed);
            if (quickCompleteButton != null) quickCompleteButton.onClick.AddListener(HandleQuickCompletePressed);

            Refresh();
        }

        public void Refresh()
        {
            if (expeditionManager == null || string.IsNullOrEmpty(slotId)) return;

            bool isComplete = expeditionManager.IsComplete(slotId);
            var remaining = expeditionManager.GetRemainingTime(slotId);

            if (timerText != null)
                timerText.text = isComplete ? "Complete!" : $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";

            if (claimButton != null) claimButton.gameObject.SetActive(isComplete);
            if (quickCompleteButton != null) quickCompleteButton.gameObject.SetActive(!isComplete);
        }

        private void HandleClaimPressed()
        {
            expeditionManager?.TryClaim(slotId);
            Refresh();
        }

        private void HandleQuickCompletePressed()
        {
            if (expeditionManager == null || currencyManager == null) return;
            if (!expeditionManager.ActiveExpeditions.TryGetValue(slotId, out var expedition)) return;

            int minutesRemaining = Mathf.CeilToInt((float)expeditionManager.GetRemainingTime(slotId).TotalMinutes);
            int cost = minutesRemaining * GemsPerMinuteSkipped;
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gems, cost)) return;

            expedition.endTimeUnix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Refresh();
        }
    }
}
