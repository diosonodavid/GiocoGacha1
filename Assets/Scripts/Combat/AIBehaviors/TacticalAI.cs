using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Combat.AIBehaviors
{
    // Targets the enemy party's highest-ATK unit (the presumed main DPS) for debuffs. Also exposes
    // SelectAllyToProtect (lowest-HP ally) as a secondary, non-abstract query for callers that
    // support shielding/taunting - not part of the base contract since Aggressive/Healer have no use for it.
    [CreateAssetMenu(fileName = "TacticalAI", menuName = "GachaGame/Combat/AI/Tactical AI")]
    public class TacticalAI : AIBaseBehavior
    {
        public override CombatTarget? SelectTarget(CharacterInstance self, IReadOnlyList<CombatTarget> allies, IReadOnlyList<CombatTarget> enemies)
        {
            CombatTarget? highestAtkEnemy = null;
            float highestAtk = float.NegativeInfinity;

            foreach (var enemy in enemies)
            {
                if (enemy.Combatant == null || !enemy.Combatant.IsAlive) continue;

                float atk = enemy.Combatant.BaseStats != null && enemy.Combatant.BaseStats.TryGetValue(StatType.ATK, out var value) ? value : 0f;
                if (atk > highestAtk)
                {
                    highestAtk = atk;
                    highestAtkEnemy = enemy;
                }
            }

            return highestAtkEnemy;
        }

        public CombatTarget? SelectAllyToProtect(IReadOnlyList<CombatTarget> allies)
        {
            CombatTarget? lowestHpAlly = null;
            int lowestHp = int.MaxValue;

            foreach (var ally in allies)
            {
                if (ally.Combatant == null || !ally.Combatant.IsAlive) continue;
                if (ally.Combatant.CurrentHp < lowestHp)
                {
                    lowestHp = ally.Combatant.CurrentHp;
                    lowestHpAlly = ally;
                }
            }

            return lowestHpAlly;
        }
    }
}
