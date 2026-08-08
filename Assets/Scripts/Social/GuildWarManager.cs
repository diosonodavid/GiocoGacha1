using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Social
{
    public enum GuildWarPhase
    {
        Inactive,
        Preparation,
        Attack,
        Concluded
    }

    // Client-side guild war state: phase timing, per-territory control, and the running score used
    // for the season leaderboard. Client bookkeeping only - as with GuildBossManager/AntiCheatManager
    // elsewhere, the server would be the real authority on attack outcomes and score.
    public class GuildWarManager : MonoBehaviour, IService
    {
        public event Action<GuildWarPhase> OnPhaseChanged;
        public event Action<string, string> OnTerritoryCaptured; // territoryId, capturingGuildId
        public event Action<string, int> OnScoreChanged; // guildId, newScore

        private NetworkManager networkManager;
        private readonly Dictionary<string, string> controllingGuildByTerritoryId = new();
        private readonly Dictionary<string, int> scoreByGuildId = new();

        public GuildWarPhase CurrentPhase { get; private set; } = GuildWarPhase.Inactive;
        public long PhaseEndTimeUnix { get; private set; }

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Debug.Log($"{nameof(GuildWarManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void StartPreparationPhase(int durationSeconds) => SetPhase(GuildWarPhase.Preparation, durationSeconds);

        public void StartAttackPhase(int durationSeconds) => SetPhase(GuildWarPhase.Attack, durationSeconds);

        public void ConcludeWar() => SetPhase(GuildWarPhase.Concluded, 0);

        // Reports one attack's outcome for scoring. Fire-and-forget best-effort sync to the server -
        // the client applies the result locally immediately so the UI feels responsive regardless of
        // the network call's outcome.
        public async Task<bool> ReportAttackResultAsync(string territoryId, string attackingGuildId, bool won, int scoreAwarded)
        {
            if (CurrentPhase != GuildWarPhase.Attack || string.IsNullOrEmpty(territoryId)) return false;

            if (won)
            {
                controllingGuildByTerritoryId[territoryId] = attackingGuildId;
                OnTerritoryCaptured?.Invoke(territoryId, attackingGuildId);
            }

            AddScore(attackingGuildId, scoreAwarded);

            if (networkManager == null) return won;

            var response = await networkManager.PostAsync<object>("/guildwar/attack-result", new AttackResultBody
            {
                territoryId = territoryId,
                attackingGuildId = attackingGuildId,
                won = won,
                scoreAwarded = scoreAwarded
            });
            return response.success;
        }

        public string GetControllingGuild(string territoryId) =>
            territoryId != null && controllingGuildByTerritoryId.TryGetValue(territoryId, out var guildId) ? guildId : null;

        public int GetScore(string guildId) =>
            guildId != null && scoreByGuildId.TryGetValue(guildId, out var score) ? score : 0;

        private void AddScore(string guildId, int amount)
        {
            if (string.IsNullOrEmpty(guildId) || amount == 0) return;

            int newScore = GetScore(guildId) + amount;
            scoreByGuildId[guildId] = newScore;
            OnScoreChanged?.Invoke(guildId, newScore);
        }

        private void SetPhase(GuildWarPhase phase, int durationSeconds)
        {
            CurrentPhase = phase;
            PhaseEndTimeUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + durationSeconds;
            OnPhaseChanged?.Invoke(phase);
        }

        [Serializable]
        private class AttackResultBody
        {
            public string territoryId;
            public string attackingGuildId;
            public bool won;
            public int scoreAwarded;
        }
    }
}
