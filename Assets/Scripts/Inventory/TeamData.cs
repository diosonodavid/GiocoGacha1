using System;

namespace GachaGame.Inventory
{
    // Serializable snapshot of one saved team loadout: up to MaxSlots owned characters
    // (identified by baseDataId, matching InventoryManager.OwnedCharacters' keying), with slot 0
    // treated as the Leader slot.
    [Serializable]
    public class TeamData
    {
        public const int MaxSlots = 4;
        public const int LeaderSlotIndex = 0;

        public string teamId;
        public string teamName;
        public string[] memberBaseDataIds = new string[MaxSlots];

        public bool IsSlotEmpty(int slotIndex) =>
            slotIndex < 0 || slotIndex >= MaxSlots || string.IsNullOrEmpty(memberBaseDataIds[slotIndex]);

        public string GetLeaderBaseDataId() => memberBaseDataIds[LeaderSlotIndex];
    }
}
