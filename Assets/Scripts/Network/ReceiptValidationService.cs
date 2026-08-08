using System;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Network
{
    [Serializable]
    public class ReceiptValidationRequest
    {
        public string productId;
        public string receiptJson;
        public string signature;
    }

    [Serializable]
    public class ReceiptValidationResponse
    {
        public bool isValid;
        public string reason;
    }

    // Server-side receipt verification against the platform store's own validation endpoint - a
    // client can fabricate any receipt locally, so IAPManager only grants currency after this
    // returns isValid from the backend, never from a local check.
    public class ReceiptValidationService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Debug.Log($"{nameof(ReceiptValidationService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<bool> ValidateReceiptAsync(string productId, string receiptJson, string signature)
        {
            if (networkManager == null) return false;

            var request = new ReceiptValidationRequest { productId = productId, receiptJson = receiptJson, signature = signature };
            var response = await networkManager.PostAsync<ReceiptValidationResponse>("/iap/validate-receipt", request);
            return response.success && response.data != null && response.data.isValid;
        }
    }
}
