using UnityEngine;

namespace GachaGame.Data
{
    public enum FurnitureCategory
    {
        Seating,
        Storage,
        Decoration,
        Lighting,
        Bedding
    }

    [CreateAssetMenu(fileName = "FurnitureData", menuName = "GachaGame/Data/Furniture Data")]
    public class FurnitureData : ScriptableObject
    {
        public string furnitureId;
        public string furnitureName;
        public FurnitureCategory category;
        public int comfortPoints;
        public Sprite icon;
        public GameObject prefab;
    }
}
