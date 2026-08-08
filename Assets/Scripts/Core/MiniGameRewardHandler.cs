using System.Threading.Tasks;
using GachaGame.Managers;
using GachaGame.MiniGames;
using UnityEngine;

namespace GachaGame.Core
{
    // Credits a finished mini-game's rewards into InventoryManager's material ledger. Kept separate
    // from BaseMiniGame itself so individual mini-games stay reusable outside a full player-inventory
    // context (e.g. a demo scene) - callers pass the MiniGameResult explicitly once a game ends.
    public class MiniGameRewardHandler : MonoBehaviour, IService
    {
        private InventoryManager inventoryManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            Debug.Log($"{nameof(MiniGameRewardHandler)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void GrantRewards(MiniGameResult result)
        {
            if (inventoryManager == null || result?.rewards == null) return;

            foreach (var reward in result.rewards)
            {
                if (reward?.item == null || reward.amount <= 0) continue;
                inventoryManager.AddMaterial(reward.item.itemId, reward.amount);
            }
        }
    }
}
