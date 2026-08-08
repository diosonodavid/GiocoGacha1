using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Networking;
using GachaGame.Social;
using UnityEngine;

namespace GachaGame.Core
{
    [Serializable]
    public class ArenaMatchResultDto
    {
        public int newArenaPoints;
        public int pointsDelta;
        public List<StageReward> rewards = new();
    }

    // Owns the player's asynchronous-PvP state (points, rank, win/loss record, current attackable
    // opponents) as a proper service, unlike the pre-existing PvPUI which talks to NetworkManager
    // directly for a simpler leaderboard view - ArenaUI/ArenaResultUI bind to this instead so the
    // state (points, rank) survives outside any one screen's lifetime and can be saved.
    public class ArenaManager : MonoBehaviour, IService
    {
        public event Action<int> OnArenaPointsChanged;
        public event Action<IReadOnlyList<ArenaOpponentData>> OnOpponentsRefreshed;
        public event Action<ArenaRankData> OnRankChanged;

        [SerializeField] private List<ArenaRankData> ranks = new();

        private NetworkManager networkManager;
        private readonly List<ArenaOpponentData> currentOpponents = new();

        public int ArenaPoints { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public IReadOnlyList<ArenaOpponentData> CurrentOpponents => currentOpponents;

        public ArenaRankData CurrentRank =>
            ranks.Where(r => r != null && ArenaPoints >= r.minPoints)
                 .OrderByDescending(r => r.minPoints)
                 .FirstOrDefault();

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Debug.Log($"{nameof(ArenaManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void LoadState(int savedPoints, int savedWins, int savedLosses)
        {
            ArenaPoints = Mathf.Max(0, savedPoints);
            Wins = Mathf.Max(0, savedWins);
            Losses = Mathf.Max(0, savedLosses);
        }

        public async Task RefreshOpponentsAsync()
        {
            if (networkManager == null) return;

            var response = await networkManager.GetAsync<List<ArenaOpponentData>>("/arena/opponents");
            if (!response.success || response.data == null) return;

            currentOpponents.Clear();
            currentOpponents.AddRange(response.data);
            OnOpponentsRefreshed?.Invoke(currentOpponents);
        }

        // Reports the outcome of an attack to the backend (the source of truth for the actual
        // point delta) and applies whatever it returns locally.
        public async Task<ArenaMatchResultDto> ReportMatchResultAsync(string opponentId, bool attackerWon)
        {
            if (networkManager == null || string.IsNullOrEmpty(opponentId)) return null;

            var body = new ArenaReportBody { opponentId = opponentId, attackerWon = attackerWon };
            var response = await networkManager.PostAsync<ArenaMatchResultDto>("/arena/report-result", body);
            if (!response.success || response.data == null) return null;

            var previousRank = CurrentRank;

            ArenaPoints = response.data.newArenaPoints;
            if (attackerWon) Wins++; else Losses++;
            OnArenaPointsChanged?.Invoke(ArenaPoints);

            if (CurrentRank != previousRank) OnRankChanged?.Invoke(CurrentRank);

            return response.data;
        }

        [Serializable]
        private class ArenaReportBody
        {
            public string opponentId;
            public bool attackerWon;
        }
    }
}
