using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Combat.AIBehaviors
{
    [CreateAssetMenu(fileName = "AggressiveAI", menuName = "GachaGame/Combat/AI/Aggressive AI")]
    public class AggressiveAI : AIBaseBehavior
    {
        public override CombatTarget? SelectTarget(CharacterInstance self, IReadOnlyList<CombatTarget> allies, IReadOnlyList<CombatTarget> enemies)
        {
            CombatTarget? lowestHpTarget = null;
            int lowestHp = int.MaxValue;

            foreach (var enemy in enemies)
            {
                if (enemy.Combatant == null || !enemy.Combatant.IsAlive) continue;
                if (enemy.Combatant.CurrentHp < lowestHp)
                {
                    lowestHp = enemy.Combatant.CurrentHp;
                    lowestHpTarget = enemy;
                }
            }

            return lowestHpTarget;
        }
    }
}
