using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Consumes 3 owned gems sharing the same level and socket color, and mints a new higher-level
    // gem instance via ScriptableObject.CreateInstance (same runtime-instance convention as
    // GemData/RuneData) rather than looking up a pre-authored "next level" catalog asset.
    public class GemCombineService : MonoBehaviour, IService
    {
        public const int GemsRequiredToCombine = 3;
        private const float StatBonusGrowthPerLevel = 1.5f;

        public event Action<GemData> OnGemsCombined;

        private InventoryManager inventoryManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            Debug.Log($"{nameof(GemCombineService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool CanCombine(List<GemData> gems)
        {
            if (gems == null || gems.Count != GemsRequiredToCombine || inventoryManager == null) return false;

            foreach (var gem in gems)
            {
                if (gem == null || !inventoryManager.OwnedGems.Contains(gem)) return false;
                if (gem.gemLevel != gems[0].gemLevel || gem.socketColor != gems[0].socketColor) return false;
            }

            return true;
        }

        public GemData TryCombine(List<GemData> gems)
        {
            if (!CanCombine(gems)) return null;

            foreach (var gem in gems)
                inventoryManager.RemoveGem(gem);

            var source = gems[0];
            var combined = ScriptableObject.CreateInstance<GemData>();
            combined.itemId = $"{source.itemId}_lv{source.gemLevel + 1}_{Guid.NewGuid():N}";
            combined.itemName = source.itemName;
            combined.description = source.description;
            combined.icon = source.icon;
            combined.itemType = source.itemType;
            combined.rarity = source.rarity;
            combined.socketColor = source.socketColor;
            combined.gemLevel = source.gemLevel + 1;
            combined.statBonus = source.statBonus != null
                ? new GearStat { statType = source.statBonus.statType, value = source.statBonus.value * StatBonusGrowthPerLevel, isPercentage = source.statBonus.isPercentage }
                : null;

            inventoryManager.AddGem(combined);
            OnGemsCombined?.Invoke(combined);
            return combined;
        }
    }
}
