using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Managers
{
    public static class StatusEffectManager
    {
        public static void ApplyEffect(ICombatant target, StatusEffectData effectData)
        {
            if (target == null || effectData == null) return;

            var modifier = effectData.modifier;
            modifier.durationTurns = effectData.duration;
            target.AddStatusEffect(modifier);
        }

        public static void RemoveEffect(ICombatant target, StatType stat) => target?.RemoveStatusEffect(stat);

        public static void TickDurations(ICombatant target) => target?.TickStatusEffectDurations();

        // Poison/Burn are modeled as a negative, percentage-of-max-HP StatModifier on StatType.HP;
        // this resolves that damage at the start of the combatant's turn.
        public static int ApplyStartOfTurnDotDamage(ICombatant target)
        {
            if (target == null) return 0;

            int totalDamage = 0;
            foreach (var effect in target.ActiveStatusEffects)
            {
                if (effect.stat != StatType.HP || !effect.isPercentage || effect.value >= 0f)
                    continue;
                totalDamage += Mathf.RoundToInt(target.MaxHp * (-effect.value / 100f));
            }

            if (totalDamage > 0)
                target.ApplyDamage(totalDamage);

            return totalDamage;
        }
    }
}
