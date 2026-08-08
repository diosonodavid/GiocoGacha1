using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class FactionShopUI : UIController
    {
        [SerializeField] private FactionShopData shopData;
        [SerializeField] private Transform entryListContainer;
        [SerializeField] private GameObject entryPrefab;

        private FactionReputationManager reputationManager;
        private CurrencyManager currencyManager;
        private InventoryManager inventoryManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out reputationManager);
            ServiceLocator.Instance.TryGet(out currencyManager);
            ServiceLocator.Instance.TryGet(out inventoryManager);
            RebuildList();
        }

        private void RebuildList()
        {
            ClearContainer();
            if (shopData == null || entryListContainer == null || entryPrefab == null) return;

            foreach (var entry in shopData.entries)
            {
                if (entry?.reward?.item == null) continue;

                bool unlocked = reputationManager == null || reputationManager.HasReachedRank(shopData.factionId, entry.requiredRankIndex);

                var entryObject = Instantiate(entryPrefab, entryListContainer);
                var label = entryObject.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = unlocked
                        ? $"{entry.reward.item.itemName} x{entry.reward.amount} - {entry.costAmount} {entry.costCurrency}"
                        : $"Locked (rank {entry.requiredRankIndex + 1})";
                }

                var button = entryObject.GetComponentInChildren<Button>();
                if (button != null)
                {
                    button.interactable = unlocked;
                    button.onClick.AddListener(() => HandlePurchase(entry));
                }
            }
        }

        private void HandlePurchase(FactionShopEntry entry)
        {
            if (currencyManager == null || inventoryManager == null || entry?.reward?.item == null) return;
            if (!currencyManager.TrySpendCurrency(entry.costCurrency, entry.costAmount)) return;

            inventoryManager.AddMaterial(entry.reward.item.itemId, entry.reward.amount);
            RebuildList();
        }

        private void ClearContainer()
        {
            if (entryListContainer == null) return;
            for (int i = entryListContainer.childCount - 1; i >= 0; i--)
                Destroy(entryListContainer.GetChild(i).gameObject);
        }
    }
}
