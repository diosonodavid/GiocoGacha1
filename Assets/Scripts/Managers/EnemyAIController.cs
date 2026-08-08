using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Managers
{
    public static class EnemyAIController
    {
        public static SkillData SelectSkill(CharacterInstance enemy, IEnumerable<SkillData> availableSkills)
        {
            if (enemy == null || availableSkills == null) return null;

            SkillData best = null;
            foreach (var skill in availableSkills)
            {
                if (skill == null || enemy.GetSkillCooldown(skill.skillId) > 0) continue;
                if (best == null || skill.damageMultiplier > best.damageMultiplier)
                    best = skill;
            }

            return best;
        }

        // Prioritizes the lowest-HP target, breaking ties in favor of an elemental weakness.
        public static CombatTarget? SelectTarget(ElementType attackerElement, IEnumerable<CombatTarget> candidates)
        {
            if (candidates == null) return null;

            CombatTarget? best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var candidate in candidates)
            {
                if (candidate.Combatant == null || !candidate.Combatant.IsAlive) continue;

                float score = ScoreTarget(attackerElement, candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private static float ScoreTarget(ElementType attackerElement, CombatTarget candidate)
        {
            float maxHp = Mathf.Max(1, candidate.Combatant.MaxHp);
            float hpRatio = candidate.Combatant.CurrentHp / maxHp;

            DamageCalculator.GetElementFactor(attackerElement, candidate.Element, out bool isWeakness);

            return (1f - hpRatio) + (isWeakness ? 1f : 0f);
        }
    }
}
