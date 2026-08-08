using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class FactionShopEntry
    {
        public RecipeIngredient reward;
        public int requiredRankIndex;
        public CurrencyType costCurrency;
        public int costAmount;
    }

    [CreateAssetMenu(fileName = "FactionShopData", menuName = "GachaGame/Data/Faction Shop Data")]
    public class FactionShopData : ScriptableObject
    {
        public string factionId;
        public List<FactionShopEntry> entries = new();
    }
}
