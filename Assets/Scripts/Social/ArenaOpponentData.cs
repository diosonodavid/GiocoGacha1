using System;

namespace GachaGame.Social
{
    // Client-side snapshot of an asynchronous PvP defender: enough to render a preview and attack
    // without the opponent needing to be online. Slot layout mirrors TeamData's convention (slot 0
    // = leader) even though this isn't a TeamData itself, since the opponent's characters aren't in
    // the local player's InventoryManager.
    [Serializable]
    public class ArenaOpponentData
    {
        public const int MaxSlots = 4;
        public const int LeaderSlotIndex = 0;

        public string opponentId;
        public string displayName;
        public int accountLevel;
        public int arenaPoints;
        public int totalPower;
        public string[] defenseTeamBaseDataIds = new string[MaxSlots];

        public string GetLeaderBaseDataId() => defenseTeamBaseDataIds[LeaderSlotIndex];
    }
}
