using System;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class LimitedOfferUI : UIController
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text countdownText;
        [SerializeField] private Transform itemListContainer;
        [SerializeField] private GameObject itemEntryPrefab;
        [SerializeField] private Button purchaseButton;

        private LimitedOfferManager offerManager;
        private LimitedOfferData boundOffer;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out offerManager);

            if (purchaseButton != null) purchaseButton.onClick.AddListener(HandlePurchasePressed);
            if (offerManager != null) offerManager.OnOfferExpired += HandleOfferExpired;
        }

        protected override void OnHidden()
        {
            if (purchaseButton != null) purchaseButton.onClick.RemoveListener(HandlePurchasePressed);
            if (offerManager != null) offerManager.OnOfferExpired -= HandleOfferExpired;
        }

        public void ShowOffer(LimitedOfferData offer)
        {
            boundOffer = offer;
            if (boundOffer == null) return;

            if (titleText != null) titleText.text = boundOffer.offerId;
            if (priceText != null) priceText.text = $"{boundOffer.price} {boundOffer.priceCurrency}";

            RebuildItemList();
            Show();
        }

        private void Update()
        {
            if (!IsShown || boundOffer == null || countdownText == null || offerManager == null) return;

            var remaining = offerManager.GetRemainingTime(boundOffer.offerId);
            if (remaining <= TimeSpan.Zero)
            {
                Hide();
                return;
            }

            countdownText.text = remaining.ToString(@"hh\:mm\:ss");
        }

        private void RebuildItemList()
        {
            if (itemListContainer == null || boundOffer == null) return;

            for (int i = itemListContainer.childCount - 1; i >= 0; i--)
                Destroy(itemListContainer.GetChild(i).gameObject);

            foreach (var entry in boundOffer.itemsContained)
            {
                if (entry.item == null || itemEntryPrefab == null) continue;

                var go = Instantiate(itemEntryPrefab, itemListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{entry.item.itemName} x{entry.amount}";
            }
        }

        private void HandlePurchasePressed()
        {
            if (offerManager == null || boundOffer == null) return;
            if (offerManager.TryPurchase(boundOffer.offerId)) Hide();
        }

        private void HandleOfferExpired(LimitedOfferData offer)
        {
            if (offer == boundOffer) Hide();
        }
    }
}
