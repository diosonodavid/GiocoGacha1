using System.Collections.Generic;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Utils
{
    public static class StatCalculator
    {
        private const float PerLevelGrowth = 0.02f; // +2% of base stat per level above 1

        public static Dictionary<StatType, float> CalculateFinalStats(
            IReadOnlyDictionary<StatType, float> baseStats,
            int level,
            IEnumerable<GearData> equippedGear = null,
            IEnumerable<StatModifier> activeBuffs = null)
        {
            var result = new Dictionary<StatType, float>();
            foreach (var kvp in baseStats)
                result[kvp.Key] = ApplyLevelGrowth(kvp.Value, level);

            if (equippedGear != null)
                ApplyGear(result, equippedGear);

            if (activeBuffs != null)
                ApplyBuffs(result, activeBuffs);

            return result;
        }

        private static float ApplyLevelGrowth(float baseValue, int level)
        {
            int clampedLevel = Mathf.Clamp(level, 1, GameConstants.MaxLevel);
            return baseValue * (1f + PerLevelGrowth * (clampedLevel - 1));
        }

        private static void ApplyGear(Dictionary<StatType, float> stats, IEnumerable<GearData> gear)
        {
            var flatBonuses = new Dictionary<StatType, float>();
            var percentBonuses = new Dictionary<StatType, float>();

            foreach (var item in gear)
            {
                if (item == null) continue;
                AccumulateStat(item.mainStat, flatBonuses, percentBonuses);
                if (item.subStats == null) continue;
                foreach (var sub in item.subStats)
                    AccumulateStat(sub, flatBonuses, percentBonuses);
            }

            foreach (var kvp in flatBonuses)
                stats[kvp.Key] = GetOrZero(stats, kvp.Key) + kvp.Value;

            foreach (var kvp in percentBonuses)
            {
                float baseValue = GetOrZero(stats, kvp.Key);
                stats[kvp.Key] = baseValue + baseValue * (kvp.Value / 100f);
            }
        }

        private static void AccumulateStat(GearStat stat, Dictionary<StatType, float> flat, Dictionary<StatType, float> percent)
        {
            if (stat == null) return;
            var target = stat.isPercentage ? percent : flat;
            target[stat.statType] = GetOrZero(target, stat.statType) + stat.value;
        }

        private static void ApplyBuffs(Dictionary<StatType, float> stats, IEnumerable<StatModifier> buffs)
        {
            foreach (var buff in buffs)
            {
                float baseValue = GetOrZero(stats, buff.stat);
                float delta = buff.isPercentage ? baseValue * (buff.value / 100f) : buff.value;
                stats[buff.stat] = baseValue + delta;
            }
        }

        private static float GetOrZero(Dictionary<StatType, float> dict, StatType key) =>
            dict.TryGetValue(key, out var value) ? value : 0f;
    }
}
