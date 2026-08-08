using System;

namespace GachaGame.Data
{
    [Serializable]
    public struct GachaHistoryEntry
    {
        public string characterId;
        public string characterName;
        public Rarity rarity;
        public bool isNew;
        public long timestampUnix;
    }
}
