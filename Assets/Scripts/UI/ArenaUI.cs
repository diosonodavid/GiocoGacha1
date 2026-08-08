using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Inventory;
using GachaGame.Networking;
using GachaGame.Social;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    [Serializable]
    public class ArenaLeaderboardEntryDto
    {
        public int rank;
        public string displayName;
        public int arenaPoints;
    }

    // Arena front screen: shows the player's own defense team (via TeamManager, so it always
    // reflects whatever TeamUI last saved), the current attackable opponent pool and rank (via
    // ArenaManager), and a read-only Top 100 leaderboard fetched directly - the same pattern
    // PvPUI already uses for that kind of read-only data.
    public class ArenaUI : UIController
    {
        [SerializeField] private float leaderboardRefreshIntervalSeconds = 15f;
        [SerializeField] private Text defenseTeamSummaryText;
        [SerializeField] private Text rankText;
        [SerializeField] private Transform opponentListContainer;
        [SerializeField] private GameObject opponentEntryPrefab;
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        private ArenaManager arenaManager;
        private TeamManager teamManager;
        private NetworkManager networkManager;
        private float leaderboardTimer;

        public event Action<ArenaOpponentData> OnOpponentSelected;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out arenaManager);
            ServiceLocator.Instance.TryGet(out teamManager);
            ServiceLocator.Instance.TryGet(out networkManager);

            if (arenaManager != null)
            {
                arenaManager.OnOpponentsRefreshed += HandleOpponentsRefreshed;
                arenaManager.OnRankChanged += HandleRankChanged;
            }

            leaderboardTimer = 0f;
            RefreshDefenseTeamSummary();
            RefreshRankText();
            _ = arenaManager?.RefreshOpponentsAsync();
            _ = RefreshLeaderboardAsync();
        }

        protected override void OnHidden()
        {
            if (arenaManager != null)
            {
                arenaManager.OnOpponentsRefreshed -= HandleOpponentsRefreshed;
                arenaManager.OnRankChanged -= HandleRankChanged;
            }
        }

        private void Update()
        {
            if (!IsShown) return;

            leaderboardTimer += Time.deltaTime;
            if (leaderboardTimer < leaderboardRefreshIntervalSeconds) return;

            leaderboardTimer = 0f;
            _ = RefreshLeaderboardAsync();
        }

        private void RefreshDefenseTeamSummary()
        {
            if (defenseTeamSummaryText == null) return;

            var activeTeam = teamManager?.ActiveTeam;
            defenseTeamSummaryText.text = activeTeam != null ? activeTeam.teamName : "No defense team set";
        }

        private void RefreshRankText()
        {
            if (rankText == null || arenaManager == null) return;

            var rank = arenaManager.CurrentRank;
            rankText.text = rank != null ? $"{rank.rankName}  ({arenaManager.ArenaPoints} pts)" : $"{arenaManager.ArenaPoints} pts";
        }

        private void HandleRankChanged(GachaGame.Data.ArenaRankData newRank) => RefreshRankText();

        private void HandleOpponentsRefreshed(IReadOnlyList<ArenaOpponentData> opponents)
        {
            if (opponentListContainer == null || opponentEntryPrefab == null) return;

            for (int i = opponentListContainer.childCount - 1; i >= 0; i--)
                Destroy(opponentListContainer.GetChild(i).gameObject);

            foreach (var opponent in opponents)
                BuildOpponentEntry(opponent);
        }

        private void BuildOpponentEntry(ArenaOpponentData opponent)
        {
            var go = Instantiate(opponentEntryPrefab, opponentListContainer);

            var label = go.GetComponentInChildren<Text>();
            if (label != null) label.text = $"{opponent.displayName}  Lv.{opponent.accountLevel}  Power {opponent.totalPower:N0}";

            var button = go.GetComponentInChildren<Button>();
            if (button != null) button.onClick.AddListener(() => OnOpponentSelected?.Invoke(opponent));
        }

        private async Task RefreshLeaderboardAsync()
        {
            if (networkManager == null || leaderboardContainer == null || leaderboardEntryPrefab == null) return;

            var response = await networkManager.GetAsync<List<ArenaLeaderboardEntryDto>>("/arena/leaderboard");
            if (!response.success || response.data == null) return;

            for (int i = leaderboardContainer.childCount - 1; i >= 0; i--)
                Destroy(leaderboardContainer.GetChild(i).gameObject);

            foreach (var entry in response.data)
            {
                var go = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"#{entry.rank}  {entry.displayName}  {entry.arenaPoints} pts";
            }
        }
    }
}
