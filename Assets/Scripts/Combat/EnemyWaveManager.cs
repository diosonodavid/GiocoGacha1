using System;
using System.Collections.Generic;
using GachaGame.Core;

namespace GachaGame.Combat
{
    // Orchestrates multi-wave encounters by re-starting BattleTurnController with the next wave's
    // enemies whenever the current wave is fully cleared (Victory), until every wave is exhausted.
    // Player-party ICombatant instances (and their HP) carry over between waves unmodified - no
    // artificial full-heal between waves, matching "multiple waves within the same encounter."
    // Plain class rather than a MonoBehaviour/IService, like BattleTurnController/GuildBossManager:
    // this is ephemeral per-encounter state, not a persistent game service.
    public class EnemyWaveManager
    {
        public event Action<int> OnWaveStarted;
        public event Action OnAllWavesCleared;
        public event Action OnEncounterDefeated;

        private readonly BattleTurnController battleTurnController;
        private readonly List<List<ICombatant>> waves = new();
        private List<ICombatant> playerParty = new();

        public int CurrentWaveIndex { get; private set; } = -1;
        public int TotalWaves => waves.Count;

        public EnemyWaveManager(BattleTurnController battleTurnController)
        {
            this.battleTurnController = battleTurnController;
            battleTurnController.OnBattleEnd += HandleBattleEnd;
        }

        public void StartEncounter(IEnumerable<ICombatant> players, List<List<ICombatant>> enemyWaves)
        {
            playerParty = new List<ICombatant>(players);
            waves.Clear();
            waves.AddRange(enemyWaves);
            CurrentWaveIndex = -1;
            AdvanceWave();
        }

        private void HandleBattleEnd(BattleState result)
        {
            if (result == BattleState.Victory) AdvanceWave();
            else if (result == BattleState.Defeat) OnEncounterDefeated?.Invoke();
        }

        private void AdvanceWave()
        {
            CurrentWaveIndex++;
            if (CurrentWaveIndex >= waves.Count)
            {
                OnAllWavesCleared?.Invoke();
                return;
            }

            OnWaveStarted?.Invoke(CurrentWaveIndex);
            battleTurnController.StartBattle(playerParty, waves[CurrentWaveIndex]);
        }
    }
}
