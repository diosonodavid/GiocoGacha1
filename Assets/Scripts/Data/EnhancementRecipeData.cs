using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class RecipeIngredient
    {
        public ItemData item;
        public int amount;
    }

    [CreateAssetMenu(fileName = "EnhancementRecipeData", menuName = "GachaGame/Data/Enhancement Recipe Data")]
    public class EnhancementRecipeData : ScriptableObject
    {
        public string recipeId;
        public string recipeName;
        public List<RecipeIngredient> ingredients = new();
        public ItemData resultItem;
        public int resultAmount = 1;
        public int goldCost;
        [Range(0f, 100f)] public float successRatePercent = 100f;
    }
}
