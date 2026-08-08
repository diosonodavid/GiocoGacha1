using System;
using System.Collections.Generic;
using GachaGame.Data;
using GachaGame.Managers;

namespace GachaGame.Core
{
    public enum BattleState
    {
        BattleStart,
        PlayerTurn,
        EnemyTurn,
        TurnEnd,
        Victory,
        Defeat
    }

    public class BattleTurnController
    {
        public event Action OnBattleStart;
        public event Action<ICombatant> OnTurnChanged;
        public event Action<ICombatant> OnCharacterDied;
        public event Action<BattleState> OnBattleEnd;

        public BattleState CurrentState { get; private set; } = BattleState.BattleStart;
        public ICombatant ActiveCombatant { get; private set; }

        private readonly TurnManager turnManager = new();
        private readonly List<ICombatant> playerParty = new();
        private readonly List<ICombatant> enemyParty = new();
        private readonly HashSet<ICombatant> deadNotified = new();

        public void StartBattle(IEnumerable<ICombatant> players, IEnumerable<ICombatant> enemies)
        {
            playerParty.Clear();
            playerParty.AddRange(players);
            enemyParty.Clear();
            enemyParty.AddRange(enemies);
            deadNotified.Clear();

            var all = new List<ICombatant>(playerParty);
            all.AddRange(enemyParty);
            turnManager.Initialize(all);

            CurrentState = BattleState.BattleStart;
            OnBattleStart?.Invoke();

            AdvanceTurn();
        }

        public void EndTurn()
        {
            CurrentState = BattleState.TurnEnd;

            if (ActiveCombatant is CharacterInstance character)
                character.TickSkillCooldowns();
            ActiveCombatant?.TickStatusEffectDurations();

            AdvanceTurn();
        }

        public void AdvanceTurn()
        {
            HandleDeaths();
            if (CheckBattleEnd()) return;

            ActiveCombatant = turnManager.GetNextActor();
            if (ActiveCombatant == null)
            {
                EndBattle(BattleState.Defeat);
                return;
            }

            CurrentState = playerParty.Contains(ActiveCombatant) ? BattleState.PlayerTurn : BattleState.EnemyTurn;

            StatusEffectManager.ApplyStartOfTurnDotDamage(ActiveCombatant);

            OnTurnChanged?.Invoke(ActiveCombatant);

            HandleDeaths();
            CheckBattleEnd();
        }

        private void HandleDeaths()
        {
            HandlePartyDeaths(playerParty);
            HandlePartyDeaths(enemyParty);
        }

        private void HandlePartyDeaths(List<ICombatant> party)
        {
            foreach (var combatant in party)
            {
                if (combatant.IsAlive || deadNotified.Contains(combatant)) continue;
                deadNotified.Add(combatant);
                turnManager.RemoveCombatant(combatant);
                OnCharacterDied?.Invoke(combatant);
            }
        }

        private bool CheckBattleEnd()
        {
            bool enemiesAlive = enemyParty.Exists(c => c.IsAlive);
            if (!enemiesAlive)
            {
                EndBattle(BattleState.Victory);
                return true;
            }

            bool playersAlive = playerParty.Exists(c => c.IsAlive);
            if (!playersAlive)
            {
                EndBattle(BattleState.Defeat);
                return true;
            }

            return false;
        }

        private void EndBattle(BattleState result)
        {
            CurrentState = result;
            OnBattleEnd?.Invoke(result);
        }
    }
}
