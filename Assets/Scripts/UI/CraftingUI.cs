using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Fusion lab screen: lists available recipes and lets the player craft one, showing whether
    // the CraftingManager roll succeeded (recipes can have a success rate below 100%).
    public class CraftingUI : UIController
    {
        [SerializeField] private List<EnhancementRecipeData> recipes = new();
        [SerializeField] private Transform recipeListContainer;
        [SerializeField] private GameObject recipeEntryPrefab;
        [SerializeField] private Text resultText;

        private CraftingManager craftingManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out craftingManager);
            RefreshRecipeList();
        }

        public void RefreshRecipeList()
        {
            if (recipeListContainer == null || recipeEntryPrefab == null) return;

            for (int i = recipeListContainer.childCount - 1; i >= 0; i--)
                Destroy(recipeListContainer.GetChild(i).gameObject);

            foreach (var recipe in recipes)
            {
                var go = Instantiate(recipeEntryPrefab, recipeListContainer);

                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{recipe.recipeName} ({recipe.successRatePercent}%) - {recipe.goldCost} gold";

                var button = go.GetComponentInChildren<Button>();
                if (button != null)
                {
                    button.interactable = craftingManager != null && craftingManager.CanCraft(recipe);
                    button.onClick.AddListener(() => Craft(recipe));
                }
            }
        }

        public void Craft(EnhancementRecipeData recipe)
        {
            if (craftingManager == null) return;

            var result = craftingManager.TryCraft(recipe);
            if (resultText != null) resultText.text = result.ToString();

            RefreshRecipeList();
        }
    }
}
