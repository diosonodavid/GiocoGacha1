using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Utilities
{
    // Dev-only shortcuts (add gems, max level, instant win) wired to InGameConsole/DebugMenuUI.
    // Every entry point re-checks Debug.isDebugBuild so a stripped release build can't reach them
    // even if something still calls in.
    public static class CheatCommands
    {
        public static void AddGems(int amount)
        {
            if (!Debug.isDebugBuild) return;
            if (ServiceLocator.Instance.TryGet<CurrencyManager>(out var currencyManager))
                currencyManager.AddCurrency(CurrencyType.Gems, amount);
        }

        public static void AddGold(int amount)
        {
            if (!Debug.isDebugBuild) return;
            if (ServiceLocator.Instance.TryGet<CurrencyManager>(out var currencyManager))
                currencyManager.AddCurrency(CurrencyType.Gold, amount);
        }

        public static void RefillStamina()
        {
            if (!Debug.isDebugBuild) return;
            if (ServiceLocator.Instance.TryGet<CurrencyManager>(out var currencyManager))
                currencyManager.AddCurrency(CurrencyType.Stamina, currencyManager.MaxStamina);
        }

        public static void MaxAccountLevel()
        {
            if (!Debug.isDebugBuild) return;
            if (!ServiceLocator.Instance.TryGet<AccountLevelManager>(out var accountLevelManager)) return;

            while (accountLevelManager.Level < GameConstants.MaxLevel)
                accountLevelManager.AddExp(AccountLevelManager.GetExpRequiredForLevel(accountLevelManager.Level));
        }

        // Immediately reports a stage as cleared with full stars, without running the battle -
        // useful for progression testing further into the campaign.
        public static void InstantWinStage(StageData stage)
        {
            if (!Debug.isDebugBuild || stage == null) return;
            if (ServiceLocator.Instance.TryGet<StageManager>(out var stageManager))
                stageManager.ReportStageCleared(stage.stageId, GameConstants.MaxStarsPerStage);
        }
    }
}
