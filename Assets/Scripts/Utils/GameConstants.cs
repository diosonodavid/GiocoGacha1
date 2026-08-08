namespace GachaGame.Utils
{
    public static class GameConstants
    {
        public const int MaxLevel = 80;
        public const int MaxMemory = 6;
        public const int SoftPityThreshold = 70;
        public const int HardPityThreshold = 90;

        public const float GachaFiveStarBaseRate = 1.5f;
        public const float GachaFourStarBaseRate = 8.5f;
        public const float GachaSoftPityRateIncreasePerPull = 5f;

        public const int StaminaRegenIntervalSeconds = 360; // 1 point every 6 minutes
        public const int MaxStaminaGainPerLevel = 5;

        public const int DailyResetIntervalSeconds = 86400;
        public const int WeeklyResetIntervalSeconds = 604800;

        public const int MaxStarsPerStage = 3;
    }
}
