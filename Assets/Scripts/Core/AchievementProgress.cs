using System;

namespace GachaGame.Core
{
    // Local save state for a single AchievementData: how far the player has progressed, and
    // whether it has been unlocked and/or its reward already claimed.
    [Serializable]
    public class AchievementProgress
    {
        public string achievementId;
        public int currentValue;
        public bool isUnlocked;
        public bool isClaimed;
    }
}
