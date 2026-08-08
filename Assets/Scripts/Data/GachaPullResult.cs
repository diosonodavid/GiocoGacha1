using System;

namespace GachaGame.Data
{
    [Serializable]
    public struct GachaPullResult
    {
        public CharacterBaseData pulledCharacter;
        public Rarity rarity;
        public bool isNew;
        public bool convertedMemoryFragment;
    }
}
