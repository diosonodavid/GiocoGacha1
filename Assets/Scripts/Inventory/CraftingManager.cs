using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Inventory
{
    public enum CraftResult { Success, Failure, InsufficientResources }

    // Consumes a recipe's ingredients + gold cost, then rolls its success rate; ingredients are
    // spent either way (matching common gacha-fusion conventions) - a failed roll just withholds
    // the result item.
    public class CraftingManager : MonoBehaviour, IService
    {
        public event Action<EnhancementRecipeData, CraftResult> OnCraftCompleted;

        private InventoryManager inventoryManager;
        private CurrencyManager currencyManager;
        private readonly System.Random random = new();

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            ServiceLocator.Instance.TryGet(out currencyManager);

            Debug.Log($"{nameof(CraftingManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool CanCraft(EnhancementRecipeData recipe)
        {
            if (recipe == null || inventoryManager == null || currencyManager == null) return false;
            if (currencyManager.Gold < recipe.goldCost) return false;

            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.item == null) continue;
                if (inventoryManager.GetMaterialCount(ingredient.item.itemId) < ingredient.amount) return false;
            }

            return true;
        }

        public CraftResult TryCraft(EnhancementRecipeData recipe)
        {
            if (!CanCraft(recipe)) return CraftResult.InsufficientResources;
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gold, recipe.goldCost)) return CraftResult.InsufficientResources;

            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient.item == null) continue;
                inventoryManager.TryConsumeMaterial(ingredient.item.itemId, ingredient.amount);
            }

            bool succeeded = random.NextDouble() * 100.0 < recipe.successRatePercent;
            if (succeeded && recipe.resultItem != null)
                inventoryManager.AddMaterial(recipe.resultItem.itemId, recipe.resultAmount);

            var result = succeeded ? CraftResult.Success : CraftResult.Failure;
            OnCraftCompleted?.Invoke(recipe, result);
            return result;
        }
    }
}
