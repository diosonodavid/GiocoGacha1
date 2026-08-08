using UnityEngine;

namespace GachaGame.Data
{
    // Character-specific fragment (as opposed to InventoryManager's rarity-wide memory fragments):
    // ShardManager tracks how many of these each character has, and CharacterAscensionSystem spends
    // them via the same ItemData.itemId-keyed material lists CraftingManager and AscensionData use.
    [CreateAssetMenu(fileName = "ShardData", menuName = "GachaGame/Data/Shard Data")]
    public class ShardData : ItemData
    {
        public string linkedCharacterBaseDataId;
        public int shardsRequiredForFullCharacter = 100;
    }
}
