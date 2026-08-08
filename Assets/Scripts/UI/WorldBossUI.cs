using System;
using System.Linq;
using GachaGame.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Global boss HP bar, damage-share ranking, and event countdown. WorldBossManager is ephemeral
    // per-event state (see its own comment) so whatever screen starts the encounter hands it over
    // via BindEncounter, mirroring GuildUI.BindBossEncounter.
    public class WorldBossUI : UIController
    {
        [SerializeField] private Image hpBarImage;
        [SerializeField] private Text hpText;
        [SerializeField] private Transform rankingContainer;
        [SerializeField] private GameObject rankingEntryPrefab;
        [SerializeField] private Text timerText;

        private WorldBossManager worldBossManager;
        private long eventEndTimeUnix;

        public void BindEncounter(WorldBossManager manager, long endTimeUnix)
        {
            if (worldBossManager != null) worldBossManager.OnBossDamaged -= HandleBossDamaged;

            worldBossManager = manager;
            eventEndTimeUnix = endTimeUnix;

            if (worldBossManager != null) worldBossManager.OnBossDamaged += HandleBossDamaged;

            RefreshHpBar();
            RefreshRanking();
        }

        protected override void OnHidden()
        {
            if (worldBossManager != null) worldBossManager.OnBossDamaged -= HandleBossDamaged;
        }

        private void Update()
        {
            if (!IsShown || timerText == null) return;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long remaining = Math.Max(0, eventEndTimeUnix - now);
            timerText.text = TimeSpan.FromSeconds(remaining).ToString(@"dd\.hh\:mm\:ss");
        }

        private void HandleBossDamaged(long remainingHp)
        {
            RefreshHpBar();
            RefreshRanking();
        }

        private void RefreshHpBar()
        {
            if (worldBossManager?.ActiveBoss == null) return;

            float ratio = worldBossManager.ActiveBoss.totalHP > 0
                ? worldBossManager.RemainingHp / (float)worldBossManager.ActiveBoss.totalHP
                : 0f;

            if (hpBarImage != null) hpBarImage.fillAmount = Mathf.Clamp01(ratio);
            if (hpText != null) hpText.text = $"{worldBossManager.RemainingHp:N0} / {worldBossManager.ActiveBoss.totalHP:N0}";
        }

        private void RefreshRanking()
        {
            if (worldBossManager == null || rankingContainer == null || rankingEntryPrefab == null) return;

            ClearContainer(rankingContainer);

            var ranked = worldBossManager.DamageByPlayerId.OrderByDescending(kvp => kvp.Value);
            foreach (var entry in ranked)
            {
                var go = Instantiate(rankingEntryPrefab, rankingContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{entry.Key}: {entry.Value:N0}";
            }
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
