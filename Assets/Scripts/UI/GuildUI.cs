using System.Linq;
using GachaGame.Combat;
using GachaGame.Core;
using GachaGame.Social;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Guild overview: current guild info and chat (from GuildManager), plus a damage leaderboard
    // for the active guild boss encounter (from GuildBossManager, when one is bound).
    public class GuildUI : UIController
    {
        [SerializeField] private Text guildNameText;
        [SerializeField] private Text guildLevelText;
        [SerializeField] private Text memberCountText;
        [SerializeField] private Transform chatMessageContainer;
        [SerializeField] private GameObject chatMessagePrefab;
        [SerializeField] private Transform damageLeaderboardContainer;
        [SerializeField] private GameObject damageLeaderboardEntryPrefab;

        private GuildManager guildManager;
        private GuildBossManager guildBossManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out guildManager);

            if (guildManager != null)
            {
                guildManager.OnGuildChanged += HandleGuildChanged;
                guildManager.OnChatMessageReceived += HandleChatMessageReceived;
            }

            RefreshGuildInfo();
            RefreshChatHistory();
        }

        protected override void OnHidden()
        {
            if (guildManager == null) return;
            guildManager.OnGuildChanged -= HandleGuildChanged;
            guildManager.OnChatMessageReceived -= HandleChatMessageReceived;
        }

        // GuildBossManager is ephemeral per-encounter state (see its own comment), so whatever
        // screen starts an encounter hands it over here rather than this screen resolving it itself.
        public void BindBossEncounter(GuildBossManager bossManager)
        {
            guildBossManager = bossManager;
            RefreshDamageLeaderboard();
        }

        private void HandleGuildChanged(GuildData guild) => RefreshGuildInfo();

        private void HandleChatMessageReceived(GuildChatMessageDto message) => RefreshChatHistory();

        private void RefreshGuildInfo()
        {
            var guild = guildManager?.CurrentGuild;

            if (guildNameText != null) guildNameText.text = guild != null ? guild.guildName : string.Empty;
            if (guildLevelText != null) guildLevelText.text = guild != null ? $"Lv. {guild.level}" : string.Empty;
            if (memberCountText != null) memberCountText.text = guild != null ? $"{guild.memberCount} members" : string.Empty;
        }

        private void RefreshChatHistory()
        {
            if (guildManager == null || chatMessageContainer == null || chatMessagePrefab == null) return;

            ClearContainer(chatMessageContainer);

            foreach (var message in guildManager.ChatHistory)
            {
                var go = Instantiate(chatMessagePrefab, chatMessageContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{message.senderId}: {message.message}";
            }
        }

        private void RefreshDamageLeaderboard()
        {
            if (guildBossManager == null || damageLeaderboardContainer == null || damageLeaderboardEntryPrefab == null) return;

            ClearContainer(damageLeaderboardContainer);

            var ranked = guildBossManager.DamageByMemberId.OrderByDescending(kvp => kvp.Value);
            foreach (var entry in ranked)
            {
                var go = Instantiate(damageLeaderboardEntryPrefab, damageLeaderboardContainer);
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
