using System;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class SeasonOverviewUI : UIController
    {
        [SerializeField] private Text seasonNameText;
        [SerializeField] private Text rulesText;
        [SerializeField] private Text countdownText;

        private SeasonManager seasonManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out seasonManager);
            Refresh();
        }

        private void Update()
        {
            if (!IsShown || seasonManager == null) return;
            RefreshCountdown();
        }

        private void Refresh()
        {
            SeasonData season = seasonManager?.CurrentSeason;
            if (season == null) return;

            if (seasonNameText != null) seasonNameText.text = season.seasonName;
            if (rulesText != null) rulesText.text = season.rulesDescription;
            RefreshCountdown();
        }

        private void RefreshCountdown()
        {
            if (countdownText == null || seasonManager == null) return;

            var remaining = TimeSpan.FromSeconds(seasonManager.SecondsRemaining);
            countdownText.text = seasonManager.IsActive ? remaining.ToString(@"dd\:hh\:mm\:ss") : "Season Ended";
        }
    }
}
