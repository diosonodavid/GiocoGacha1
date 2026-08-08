using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "GachaGame/Data/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string itemId;
        public string itemName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemType itemType;
        public Rarity rarity;
    }
}
