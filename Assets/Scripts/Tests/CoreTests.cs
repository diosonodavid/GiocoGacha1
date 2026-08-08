using System.Collections.Generic;
using GachaGame.Data;
using GachaGame.Utils;
using NUnit.Framework;

namespace GachaGame.Tests
{
    public class CoreTests
    {
        [Test]
        public void CalculateFinalStats_LevelIncreasesBaseStat()
        {
            var baseStats = new Dictionary<StatType, float> { { StatType.ATK, 100f } };

            var statsAtLevelOne = StatCalculator.CalculateFinalStats(baseStats, 1);
            var statsAtMaxLevel = StatCalculator.CalculateFinalStats(baseStats, GameConstants.MaxLevel);

            Assert.Greater(statsAtMaxLevel[StatType.ATK], statsAtLevelOne[StatType.ATK]);
        }

        [Test]
        public void CalculateFinalStats_AppliesFlatGearBonus()
        {
            var baseStats = new Dictionary<StatType, float> { { StatType.HP, 1000f } };
            var gear = new List<GearData>
            {
                new()
                {
                    mainStat = new GearStat { statType = StatType.HP, value = 200f, isPercentage = false },
                    subStats = new List<GearStat>()
                }
            };

            var result = StatCalculator.CalculateFinalStats(baseStats, 1, gear);

            Assert.AreEqual(1200f, result[StatType.HP], 0.01f);
        }

        [Test]
        public void CalculateFinalStats_AppliesPercentageGearBonus()
        {
            var baseStats = new Dictionary<StatType, float> { { StatType.ATK, 100f } };
            var gear = new List<GearData>
            {
                new()
                {
                    mainStat = new GearStat { statType = StatType.ATK, value = 50f, isPercentage = true },
                    subStats = new List<GearStat>()
                }
            };

            var result = StatCalculator.CalculateFinalStats(baseStats, 1, gear);

            Assert.AreEqual(150f, result[StatType.ATK], 0.01f);
        }

        [Test]
        public void CalculateFinalStats_AppliesPercentageBuff()
        {
            var baseStats = new Dictionary<StatType, float> { { StatType.DEF, 100f } };
            var buffs = new List<StatModifier>
            {
                new() { stat = StatType.DEF, value = 20f, isPercentage = true, durationTurns = 2 }
            };

            var result = StatCalculator.CalculateFinalStats(baseStats, 1, null, buffs);

            Assert.AreEqual(120f, result[StatType.DEF], 0.01f);
        }

        [Test]
        public void CalculateFinalStats_CombinesGearAndBuffsOnTopOfLevelGrowth()
        {
            var baseStats = new Dictionary<StatType, float> { { StatType.ATK, 100f } };
            var gear = new List<GearData>
            {
                new()
                {
                    mainStat = new GearStat { statType = StatType.ATK, value = 50f, isPercentage = false },
                    subStats = new List<GearStat>()
                }
            };
            var buffs = new List<StatModifier>
            {
                new() { stat = StatType.ATK, value = 10f, isPercentage = true, durationTurns = 1 }
            };

            var resultWithoutGearOrBuffs = StatCalculator.CalculateFinalStats(baseStats, GameConstants.MaxLevel);
            var resultWithGearAndBuffs = StatCalculator.CalculateFinalStats(baseStats, GameConstants.MaxLevel, gear, buffs);

            Assert.Greater(resultWithGearAndBuffs[StatType.ATK], resultWithoutGearOrBuffs[StatType.ATK]);
        }
    }
}
