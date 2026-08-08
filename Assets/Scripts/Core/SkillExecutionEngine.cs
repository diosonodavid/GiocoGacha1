using System.Collections.Generic;
using GachaGame.Data;

namespace GachaGame.Core
{
    // Pairs a combatant with the elemental type needed for damage math, since ICombatant
    // intentionally exposes only combat stats and not the character's element.
    public readonly struct CombatTarget
    {
        public readonly ICombatant Combatant;
        public readonly ElementType Element;

        public CombatTarget(ICombatant combatant, ElementType element)
        {
            Combatant = combatant;
            Element = element;
        }
    }

    public static class SkillExecutionEngine
    {
        public static List<DamageResult> ExecuteSkill(
            CharacterInstance caster,
            ElementType casterElement,
            SkillData skill,
            IEnumerable<CombatTarget> targets,
            System.Random random = null)
        {
            var results = new List<DamageResult>();
            if (caster == null || skill == null || targets == null) return results;
            if (caster.GetSkillCooldown(skill.skillId) > 0) return results;

            float casterAtk = GetStat(caster.BaseStats, StatType.ATK);
            float casterCritRate = GetStat(caster.BaseStats, StatType.CritRate);
            float casterCritDmg = GetStat(caster.BaseStats, StatType.CritDMG);
            random ??= new System.Random();

            foreach (var target in targets)
            {
                if (target.Combatant == null || !target.Combatant.IsAlive) continue;

                float defenderDef = GetStat(target.Combatant.BaseStats, StatType.DEF);
                bool isCritical = random.NextDouble() * 100.0 < casterCritRate;

                var result = DamageCalculator.CalculateDamage(
                    casterAtk, defenderDef, skill.damageMultiplier,
                    casterElement, target.Element, isCritical, casterCritDmg, random);

                target.Combatant.ApplyDamage(result.damageAmount);

                foreach (var statusEffect in skill.statusEffects)
                {
                    if (statusEffect == null) continue;
                    var modifier = statusEffect.modifier;
                    modifier.durationTurns = statusEffect.duration;
                    target.Combatant.AddStatusEffect(modifier);
                }

                results.Add(result);
            }

            caster.SetSkillCooldown(skill.skillId, skill.cooldown);
            return results;
        }

        private static float GetStat(IReadOnlyDictionary<StatType, float> stats, StatType type) =>
            stats != null && stats.TryGetValue(type, out var value) ? value : 0f;
    }
}
