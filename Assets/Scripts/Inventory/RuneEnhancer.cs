using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Spends gold to raise a rune's refinementLevel; at each of the four substat-unlock levels
    // (+4, +8, +12, +16) a new random substat is rolled in if the rune has fewer than the max of 4,
    // mirroring the classic rune-enhancement milestone pattern.
    public class RuneEnhancer : MonoBehaviour, IService
    {
        private const int MaxRefinementLevel = 16;
        private const int MaxSubStats = 4;
        private const int EnhanceCostPerLevel = 300;
        private static readonly int[] SubstatUnlockLevels = { 4, 8, 12, 16 };

        private CurrencyManager currencyManager;
        private readonly System.Random random = new();

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            Debug.Log($"{nameof(RuneEnhancer)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool CanEnhance(RuneData rune)
        {
            if (rune == null || currencyManager == null) return false;
            if (rune.refinementLevel >= MaxRefinementLevel) return false;
            return currencyManager.Gold >= EnhanceCostPerLevel * (rune.refinementLevel + 1);
        }

        public bool TryEnhance(RuneData rune)
        {
            if (!CanEnhance(rune)) return false;

            int cost = EnhanceCostPerLevel * (rune.refinementLevel + 1);
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gold, cost)) return false;

            rune.refinementLevel++;

            if (System.Array.IndexOf(SubstatUnlockLevels, rune.refinementLevel) >= 0 && rune.subStats.Count < MaxSubStats)
                rune.subStats.Add(RollRandomSubStat());

            return true;
        }

        private GearStat RollRandomSubStat()
        {
            var statValues = (StatType[])System.Enum.GetValues(typeof(StatType));
            var statType = statValues[random.Next(statValues.Length)];
            bool isPercentage = statType is StatType.CritRate or StatType.CritDMG or StatType.EffectHit or StatType.EffectRes;
            float value = isPercentage ? random.Next(4, 13) : random.Next(10, 51);

            return new GearStat { statType = statType, value = value, isPercentage = isPercentage };
        }
    }
}
