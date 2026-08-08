using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Managers;
using GachaGame.Network;
using UnityEngine;

namespace GachaGame.Core
{
    // Store-agnostic purchase flow: this project has no IAP SDK installed (no com.unity.purchasing
    // dependency in Packages/manifest.json), so InitiatePurchase only logs the request for now -
    // wiring it to Unity IAP's Purchasing API is a follow-up once that package is added.
    // CompletePurchase is the integration point a native store callback (or a manual test call in
    // the meantime) invokes once a receipt exists; it always goes through ReceiptValidationService
    // first, so a spoofed local receipt is rejected the same as a fraudulent one.
    public class IAPManager : MonoBehaviour, IService
    {
        public event Action<IAPProductData> OnPurchaseCompleted;
        public event Action<string> OnPurchaseFailed;

        [SerializeField] private List<IAPProductData> catalog = new();

        private CurrencyManager currencyManager;
        private ReceiptValidationService receiptValidationService;

        public IReadOnlyList<IAPProductData> Catalog => catalog;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            ServiceLocator.Instance.TryGet(out receiptValidationService);
            Debug.Log($"{nameof(IAPManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public IAPProductData GetProduct(string productId) => catalog.FirstOrDefault(p => p != null && p.productId == productId);

        public void InitiatePurchase(string productId)
        {
            var product = GetProduct(productId);
            if (product == null)
            {
                OnPurchaseFailed?.Invoke(productId);
                return;
            }

            Debug.Log($"[{nameof(IAPManager)}] Purchase requested for '{productId}' (store SDK not integrated yet).");
        }

        public async void CompletePurchase(string productId, string receiptJson, string signature)
        {
            var product = GetProduct(productId);
            if (product == null || receiptValidationService == null)
            {
                OnPurchaseFailed?.Invoke(productId);
                return;
            }

            bool valid = await receiptValidationService.ValidateReceiptAsync(productId, receiptJson, signature);
            if (!valid)
            {
                OnPurchaseFailed?.Invoke(productId);
                return;
            }

            currencyManager?.AddCurrency(CurrencyType.Gems, product.gemsAwarded);
            OnPurchaseCompleted?.Invoke(product);
        }
    }
}
