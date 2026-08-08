using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    [Serializable]
    public class PvpOpponentDto
    {
        public string userId;
        public int mmr;
        public string rankTier;
        public int totalPower;
    }

    [Serializable]
    public class PvpLeaderboardEntryDto
    {
        public int rank;
        public string userId;
        public int mmr;
    }

    [Serializable]
    public class PvpMatchResultDto
    {
        public int attackerMmr;
        public int defenderMmr;
        public string attackerRankTier;
        public string defenderRankTier;
    }

    // Arena front-end: shows the player's own defense setup, fetches nearby-MMR opponents to
    // attack, and periodically refreshes the Redis-backed Top 100 leaderboard for a "live" view.
    public class PvPUI : MonoBehaviour
    {
        [SerializeField] private float leaderboardRefreshIntervalSeconds = 15f;
        [SerializeField] private Text ownDefenseSummaryText;
        [SerializeField] private Transform opponentListContainer;
        [SerializeField] private GameObject opponentEntryPrefab;
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        private NetworkManager networkManager;
        private float leaderboardTimer;

        public event Action<PvpOpponentDto> OnOpponentSelected;

        private void OnEnable()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            leaderboardTimer = 0f;
            _ = RefreshOpponentsAsync();
            _ = RefreshLeaderboardAsync();
        }

        private void Update()
        {
            leaderboardTimer += Time.deltaTime;
            if (leaderboardTimer < leaderboardRefreshIntervalSeconds) return;

            leaderboardTimer = 0f;
            _ = RefreshLeaderboardAsync();
        }

        public void SetOwnDefenseSummary(string leaderName, int totalPower)
        {
            if (ownDefenseSummaryText != null)
                ownDefenseSummaryText.text = $"{leaderName} - Power {totalPower:N0}";
        }

        public async Task RefreshOpponentsAsync()
        {
            if (networkManager == null || opponentListContainer == null || opponentEntryPrefab == null) return;

            var response = await networkManager.GetAsync<List<PvpOpponentDto>>("/pvp/opponents");
            if (!response.success || response.data == null) return;

            for (int i = opponentListContainer.childCount - 1; i >= 0; i--)
                Destroy(opponentListContainer.GetChild(i).gameObject);

            foreach (var opponent in response.data)
            {
                var go = Instantiate(opponentEntryPrefab, opponentListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{opponent.rankTier} | MMR {opponent.mmr} | Power {opponent.totalPower:N0}";

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => OnOpponentSelected?.Invoke(opponent));
            }
        }

        public async Task RefreshLeaderboardAsync()
        {
            if (networkManager == null || leaderboardContainer == null || leaderboardEntryPrefab == null) return;

            var response = await networkManager.GetAsync<List<PvpLeaderboardEntryDto>>("/pvp/leaderboard");
            if (!response.success || response.data == null) return;

            for (int i = leaderboardContainer.childCount - 1; i >= 0; i--)
                Destroy(leaderboardContainer.GetChild(i).gameObject);

            foreach (var entry in response.data)
            {
                var go = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"#{entry.rank}  {entry.userId}  {entry.mmr} MMR";
            }
        }

        public async Task<PvpMatchResultDto> SubmitMatchResultAsync(string defenderId, bool attackerWon)
        {
            if (networkManager == null) return null;

            var body = new SubmitResultBody { defenderId = defenderId, attackerWon = attackerWon };
            var response = await networkManager.PostAsync<PvpMatchResultDto>("/pvp/submit-result", body);
            return response.success ? response.data : null;
        }

        [Serializable]
        private class SubmitResultBody
        {
            public string defenderId;
            public bool attackerWon;
        }
    }
}
