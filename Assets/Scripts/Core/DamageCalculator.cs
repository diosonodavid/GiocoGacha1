using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Core
{
    public static class DamageCalculator
    {
        private const float WeaknessFactor = 1.5f;
        private const float LightDarkFactor = 1.25f;
        private const float ResistFactor = 0.75f;
        private const float NeutralFactor = 1f;
        private const float MinVariance = 0.95f;
        private const float MaxVariance = 1.05f;

        private static readonly System.Random SharedRandom = new();

        // FinalDamage = (ATK * SkillMultiplier) * (100 / (100 + DEF)) * ElementFactor * CritFactor * RandomVariance
        public static DamageResult CalculateDamage(
            float attackerAtk,
            float defenderDef,
            float skillMultiplier,
            ElementType attackerElement,
            ElementType defenderElement,
            bool isCritical,
            float critDamagePercent,
            System.Random random = null)
        {
            random ??= SharedRandom;

            float elementFactor = GetElementFactor(attackerElement, defenderElement, out bool isWeakness);
            float critFactor = isCritical ? 1f + critDamagePercent / 100f : 1f;
            float variance = MinVariance + (float)random.NextDouble() * (MaxVariance - MinVariance);

            float rawDamage = attackerAtk * skillMultiplier * (100f / (100f + defenderDef)) * elementFactor * critFactor * variance;

            return new DamageResult
            {
                damageAmount = Mathf.Max(0, Mathf.RoundToInt(rawDamage)),
                isCritical = isCritical,
                element = attackerElement,
                isWeakness = isWeakness
            };
        }

        public static float GetElementFactor(ElementType attacker, ElementType defender, out bool isWeakness)
        {
            if (IsStrongAgainst(attacker, defender))
            {
                isWeakness = true;
                return attacker is ElementType.Light or ElementType.Dark ? LightDarkFactor : WeaknessFactor;
            }

            if (IsStrongAgainst(defender, attacker))
            {
                isWeakness = false;
                return ResistFactor;
            }

            isWeakness = false;
            return NeutralFactor;
        }

        // Fire > Grass > Water > Fire, Light <-> Dark.
        private static bool IsStrongAgainst(ElementType attacker, ElementType defender)
        {
            return (attacker, defender) switch
            {
                (ElementType.Fire, ElementType.Grass) => true,
                (ElementType.Grass, ElementType.Water) => true,
                (ElementType.Water, ElementType.Fire) => true,
                (ElementType.Light, ElementType.Dark) => true,
                (ElementType.Dark, ElementType.Light) => true,
                _ => false
            };
        }
    }
}
