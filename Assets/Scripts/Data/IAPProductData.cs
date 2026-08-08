using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "IAPProductData", menuName = "GachaGame/Data/IAP Product Data")]
    public class IAPProductData : ScriptableObject
    {
        public string productId;
        public string googlePlayProductId;
        public string appStoreProductId;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public int gemsAwarded;
        public bool isSubscription;
        public string priceDisplayString; // store-localized price, populated at runtime from store data
    }
}
