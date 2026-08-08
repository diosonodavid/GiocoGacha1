using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Core
{
    public class ActiveLimitedOffer
    {
        public LimitedOfferData data;
        public long endTimeUnix;
    }

    // Activated on demand by whatever system reaches a qualifying milestone (e.g. ChapterData
    // completion), rather than polling for triggers itself - mirrors ExpeditionManager's
    // wall-clock-timestamp convention for the countdown so it survives an app restart.
    public class LimitedOfferManager : MonoBehaviour, IService
    {
        public event Action<LimitedOfferData> OnOfferActivated;
        public event Action<LimitedOfferData> OnOfferExpired;
        public event Action<LimitedOfferData> OnOfferPurchased;

        private readonly Dictionary<string, ActiveLimitedOffer> activeOffers = new();
        private CurrencyManager currencyManager;
        private InventoryManager inventoryManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            ServiceLocator.Instance.TryGet(out inventoryManager);
            Debug.Log($"{nameof(LimitedOfferManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void ActivateOffer(LimitedOfferData offer)
        {
            if (offer == null || activeOffers.ContainsKey(offer.offerId)) return;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            activeOffers[offer.offerId] = new ActiveLimitedOffer { data = offer, endTimeUnix = now + offer.durationMinutes * 60L };
            OnOfferActivated?.Invoke(offer);
        }

        public bool IsOfferActive(string offerId)
        {
            if (!activeOffers.TryGetValue(offerId, out var active)) return false;

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= active.endTimeUnix)
            {
                activeOffers.Remove(offerId);
                OnOfferExpired?.Invoke(active.data);
                return false;
            }

            return true;
        }

        public TimeSpan GetRemainingTime(string offerId)
        {
            if (!activeOffers.TryGetValue(offerId, out var active)) return TimeSpan.Zero;
            long remaining = Math.Max(0, active.endTimeUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return TimeSpan.FromSeconds(remaining);
        }

        public bool TryPurchase(string offerId)
        {
            if (!IsOfferActive(offerId) || currencyManager == null || inventoryManager == null) return false;

            var offer = activeOffers[offerId].data;
            if (!currencyManager.TrySpendCurrency(offer.priceCurrency, offer.price)) return false;

            foreach (var entry in offer.itemsContained)
            {
                if (entry.item == null) continue;
                inventoryManager.AddMaterial(entry.item.itemId, entry.amount);
            }

            activeOffers.Remove(offerId);
            OnOfferPurchased?.Invoke(offer);
            return true;
        }
    }
}
