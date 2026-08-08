using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Combat.AIBehaviors
{
    [CreateAssetMenu(fileName = "HealerAI", menuName = "GachaGame/Combat/AI/Healer AI")]
    public class HealerAI : AIBaseBehavior
    {
        public override CombatTarget? SelectTarget(CharacterInstance self, IReadOnlyList<CombatTarget> allies, IReadOnlyList<CombatTarget> enemies)
        {
            CombatTarget? lowestRatioAlly = null;
            float lowestRatio = float.MaxValue;

            foreach (var ally in allies)
            {
                if (ally.Combatant == null || !ally.Combatant.IsAlive) continue;

                float ratio = ally.Combatant.CurrentHp / (float)Mathf.Max(1, ally.Combatant.MaxHp);
                if (ratio < lowestRatio)
                {
                    lowestRatio = ratio;
                    lowestRatioAlly = ally;
                }
            }

            return lowestRatioAlly;
        }
    }
}
