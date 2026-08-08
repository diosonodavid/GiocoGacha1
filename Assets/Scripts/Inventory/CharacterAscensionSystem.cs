using System;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Inventory
{
    public enum AscensionResult { Success, InsufficientResources, MaxTierReached, InvalidRequest }

    // Advances a character's ascension tier (star grade), spending that tier's materials + gold and
    // unlocking a higher usable level cap (Limit Break). Kept separate from CraftingManager: that
    // system spends recipe ingredients to produce a result item, this one reads/writes
    // CharacterInstance.ascensionLevel directly and never touches InventoryManager's item ledger as
    // an output. Combat stat application for statPercentBonus is left to callers (e.g. a future
    // StatCalculator hook) rather than done here, since this system only owns tier progression.
    public class CharacterAscensionSystem : MonoBehaviour, IService
    {
        private const int BaseLevelCap = 20;

        public event Action<CharacterInstance, int> OnCharacterAscended;

        private InventoryManager inventoryManager;
        private CurrencyManager currencyManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            ServiceLocator.Instance.TryGet(out currencyManager);
            Debug.Log($"{nameof(CharacterAscensionSystem)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public int GetLevelCap(CharacterInstance instance, AscensionData table)
        {
            if (instance == null) return BaseLevelCap;
            if (table == null) return Mathf.Clamp(BaseLevelCap, 1, GameConstants.MaxLevel);

            var reachedTier = table.tiers
                .Where(t => t.starLevel <= instance.ascensionLevel)
                .OrderByDescending(t => t.starLevel)
                .FirstOrDefault();

            int cap = reachedTier != null ? reachedTier.levelCapUnlocked : BaseLevelCap;
            return Mathf.Clamp(cap, 1, GameConstants.MaxLevel);
        }

        public bool CanAscend(CharacterInstance instance, AscensionData table)
        {
            if (instance == null || table == null || inventoryManager == null || currencyManager == null) return false;

            var nextTier = GetNextTier(instance, table);
            if (nextTier == null) return false;

            if (currencyManager.Gold < nextTier.goldCost) return false;

            foreach (var material in nextTier.materials)
            {
                if (material.item == null) continue;
                if (inventoryManager.GetMaterialCount(material.item.itemId) < material.amount) return false;
            }

            return true;
        }

        public AscensionResult TryAscend(CharacterInstance instance, AscensionData table)
        {
            if (instance == null || table == null) return AscensionResult.InvalidRequest;

            var nextTier = GetNextTier(instance, table);
            if (nextTier == null) return AscensionResult.MaxTierReached;
            if (!CanAscend(instance, table)) return AscensionResult.InsufficientResources;
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gold, nextTier.goldCost)) return AscensionResult.InsufficientResources;

            foreach (var material in nextTier.materials)
            {
                if (material.item == null) continue;
                inventoryManager.TryConsumeMaterial(material.item.itemId, material.amount);
            }

            instance.ascensionLevel = nextTier.starLevel;
            OnCharacterAscended?.Invoke(instance, instance.ascensionLevel);
            return AscensionResult.Success;
        }

        private static AscensionTier GetNextTier(CharacterInstance instance, AscensionData table) =>
            table.tiers.FirstOrDefault(t => t.starLevel == instance.ascensionLevel + 1);
    }
}
