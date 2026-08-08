using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Refunds every talent point a character has spent, at a flat gem cost - mirrors RuneEnhancer's
    // pattern of a small IService wrapping a single CurrencyManager-gated action.
    public class TalentResetService : MonoBehaviour, IService
    {
        private const int ResetCostGems = 50;

        private CurrencyManager currencyManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            Debug.Log($"{nameof(TalentResetService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public bool CanReset(CharacterInstance instance) =>
            instance != null && instance.unlockedTalentNodeIds.Count > 0 &&
            currencyManager != null && currencyManager.Gems >= ResetCostGems;

        public bool TryReset(CharacterInstance instance)
        {
            if (!CanReset(instance)) return false;
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gems, ResetCostGems)) return false;

            instance.talentPoints += instance.unlockedTalentNodeIds.Count;
            instance.unlockedTalentNodeIds.Clear();
            return true;
        }
    }
}
