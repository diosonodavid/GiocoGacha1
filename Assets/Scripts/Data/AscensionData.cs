using System;
using System.Collections.Generic;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class AscensionTier
    {
        public int starLevel; // ascension tier reached after completing this step (1-6)
        public List<RecipeIngredient> materials = new();
        public int goldCost;
        public int levelCapUnlocked; // new max usable character level once this tier is reached
        public SerializableDictionary<StatType, float> statPercentBonus = new();
    }

    [CreateAssetMenu(fileName = "AscensionData", menuName = "GachaGame/Data/Ascension Data")]
    public class AscensionData : ScriptableObject
    {
        public Rarity rarity;
        public List<AscensionTier> tiers = new();
    }
}
