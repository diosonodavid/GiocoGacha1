using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "LimitedOfferData", menuName = "GachaGame/Data/Limited Offer Data")]
    public class LimitedOfferData : ScriptableObject
    {
        public string offerId;
        public int durationMinutes;
        public CurrencyType priceCurrency = CurrencyType.Gems;
        public int price;
        public List<RecipeIngredient> itemsContained = new();
    }
}
