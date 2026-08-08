using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public enum ShopTab { Packages, SummonTickets, CurrencyExchange }

    // Tab-switching shop front: Packages and SummonTickets both purchase through ShopManager's
    // limit-tracked ShopPackageData flow, while CurrencyExchange applies a fixed conversion rate
    // directly against CurrencyManager (gems -> gold has no purchase-limit concept to enforce).
    public class ShopUI : MonoBehaviour
    {
        private const int GemsToGoldRate = 100; // 1 gem -> 100 gold

        [SerializeField] private List<GameObject> tabPanels = new();
        [SerializeField] private Transform packageListContainer;
        [SerializeField] private Transform ticketListContainer;
        [SerializeField] private GameObject shopEntryPrefab;
        [SerializeField] private List<ShopPackageData> packages = new();
        [SerializeField] private List<ShopPackageData> summonTickets = new();
        [SerializeField] private InputField gemsToConvertInput;

        private ShopManager shopManager;
        private CurrencyManager currencyManager;

        private void OnEnable()
        {
            ServiceLocator.Instance.TryGet(out shopManager);
            ServiceLocator.Instance.TryGet(out currencyManager);
            ShowTab(ShopTab.Packages);
        }

        public void ShowTab(ShopTab tab)
        {
            for (int i = 0; i < tabPanels.Count; i++)
                tabPanels[i].SetActive(i == (int)tab);

            if (tab == ShopTab.Packages) PopulateList(packageListContainer, packages);
            else if (tab == ShopTab.SummonTickets) PopulateList(ticketListContainer, summonTickets);
        }

        private void PopulateList(Transform container, List<ShopPackageData> items)
        {
            if (container == null || shopEntryPrefab == null) return;

            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);

            foreach (var package in items)
            {
                var go = Instantiate(shopEntryPrefab, container);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{package.packageId}: {package.cost} {package.costCurrency}";

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => TryPurchase(package));
            }
        }

        public bool TryPurchase(ShopPackageData package)
        {
            if (shopManager == null || currencyManager == null) return false;
            return shopManager.TryPurchase(package, currencyManager);
        }

        public bool TryConvertGemsToGold()
        {
            if (currencyManager == null || gemsToConvertInput == null) return false;
            if (!int.TryParse(gemsToConvertInput.text, out int gemsToSpend) || gemsToSpend <= 0) return false;
            if (!currencyManager.TrySpendCurrency(CurrencyType.Gems, gemsToSpend)) return false;

            currencyManager.AddCurrency(CurrencyType.Gold, gemsToSpend * GemsToGoldRate);
            return true;
        }
    }
}
