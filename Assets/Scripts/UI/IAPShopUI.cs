using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class IAPShopUI : UIController
    {
        [SerializeField] private Transform productContainer;
        [SerializeField] private GameObject productEntryPrefab;

        private IAPManager iapManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out iapManager);
            BuildProductList();

            if (iapManager != null)
            {
                iapManager.OnPurchaseCompleted += HandlePurchaseCompleted;
                iapManager.OnPurchaseFailed += HandlePurchaseFailed;
            }
        }

        protected override void OnHidden()
        {
            if (iapManager != null)
            {
                iapManager.OnPurchaseCompleted -= HandlePurchaseCompleted;
                iapManager.OnPurchaseFailed -= HandlePurchaseFailed;
            }
        }

        private void BuildProductList()
        {
            if (productContainer == null || productEntryPrefab == null || iapManager == null) return;

            for (int i = productContainer.childCount - 1; i >= 0; i--)
                Destroy(productContainer.GetChild(i).gameObject);

            foreach (var product in iapManager.Catalog)
            {
                if (product == null) continue;

                var go = Instantiate(productEntryPrefab, productContainer);

                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{product.displayName} - {product.priceDisplayString}";

                var icon = go.GetComponentInChildren<Image>();
                if (icon != null && product.icon != null) icon.sprite = product.icon;

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => iapManager.InitiatePurchase(product.productId));
            }
        }

        private void HandlePurchaseCompleted(IAPProductData product) => Debug.Log($"[{nameof(IAPShopUI)}] Purchase completed: {product.productId}");
        private void HandlePurchaseFailed(string productId) => Debug.Log($"[{nameof(IAPShopUI)}] Purchase failed: {productId}");
    }
}
