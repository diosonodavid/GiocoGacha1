using System;
using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Combat
{
    // Drives automatic skill/target selection for the player's turn (reusing EnemyAIController's
    // heuristics - "best available skill, lowest-HP/weakest target" works equally well for an
    // auto-battle player as it does for enemy AI) and exposes battle speed multipliers via
    // Time.timeScale.
    public class AutoBattleController
    {
        private readonly BattleTurnController battleTurnController;
        private readonly Dictionary<ICombatant, IReadOnlyList<SkillData>> skillsByCombatant;
        private readonly Dictionary<ICombatant, ElementType> elementByCombatant;

        public bool IsAutoBattleEnabled { get; private set; }
        public float SpeedMultiplier { get; private set; } = 1f;

        public AutoBattleController(
            BattleTurnController battleTurnController,
            Dictionary<ICombatant, IReadOnlyList<SkillData>> skillsByCombatant,
            Dictionary<ICombatant, ElementType> elementByCombatant)
        {
            this.battleTurnController = battleTurnController;
            this.skillsByCombatant = skillsByCombatant;
            this.elementByCombatant = elementByCombatant;
        }

        public void SetAutoBattleEnabled(bool enabled) => IsAutoBattleEnabled = enabled;

        public void SetSpeedMultiplier(float multiplier)
        {
            SpeedMultiplier = Mathf.Max(1f, multiplier);
            Time.timeScale = SpeedMultiplier;
        }

        // Call on the active combatant's turn when IsAutoBattleEnabled; resolves a skill + targets
        // and executes it exactly like a manual player action would, then ends the turn.
        public bool TryExecuteAutoTurn(IEnumerable<CombatTarget> enemyTargets)
        {
            if (!IsAutoBattleEnabled) return false;

            var activeCombatant = battleTurnController.ActiveCombatant;
            if (activeCombatant is not CharacterInstance caster) return false;
            if (!skillsByCombatant.TryGetValue(activeCombatant, out var skills)) return false;

            var skill = EnemyAIController.SelectSkill(caster, skills);
            if (skill == null) return false;

            var casterElement = elementByCombatant.TryGetValue(activeCombatant, out var element) ? element : ElementType.Light;
            var targets = skill.isAoE ? enemyTargets : SelectSingleTarget(casterElement, enemyTargets);

            SkillExecutionEngine.ExecuteSkill(caster, casterElement, skill, targets);
            battleTurnController.EndTurn();
            return true;
        }

        private static IEnumerable<CombatTarget> SelectSingleTarget(ElementType casterElement, IEnumerable<CombatTarget> candidates)
        {
            var best = EnemyAIController.SelectTarget(casterElement, candidates);
            return best.HasValue ? new[] { best.Value } : Array.Empty<CombatTarget>();
        }
    }
}
