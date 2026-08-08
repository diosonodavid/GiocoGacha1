using System;

namespace GachaGame.Data
{
    [Serializable]
    public class PlayerSaveData
    {
        public string accountId;
        public int level;
        public int exp;
        public int gold;
        public int gems;
        public int stamina;
        public int maxStamina;
        public long lastStaminaUpdate;
    }
}
