using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Combat.AIBehaviors
{
    // Enemy "brain" assets: each battle-ready enemy references one of these to decide which
    // combatant it should act against. Kept decision-only (no skill execution) so callers stay free
    // to route the chosen target through SkillExecutionEngine for damage, or their own heal/buff
    // logic - this complements EnemyAIController's existing lowest-HP/weakness heuristic with named,
    // swappable per-enemy personalities rather than replacing that static helper.
    public abstract class AIBaseBehavior : ScriptableObject
    {
        public abstract CombatTarget? SelectTarget(CharacterInstance self, IReadOnlyList<CombatTarget> allies, IReadOnlyList<CombatTarget> enemies);
    }
}
