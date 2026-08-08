using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class AffinityUI : UIController
    {
        [SerializeField] private Image affinityProgressBar;
        [SerializeField] private Text affinityLevelText;
        [SerializeField] private Transform giftListContainer;
        [SerializeField] private GameObject giftEntryPrefab;
        [SerializeField] private List<GiftItemData> giftCatalog = new();

        private AffinityManager affinityManager;
        private CharacterInstance boundCharacter;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out affinityManager);
            if (affinityManager != null) affinityManager.OnAffinityLevelChanged += HandleAffinityLevelChanged;
            RebuildGiftList();
        }

        protected override void OnHidden()
        {
            if (affinityManager != null) affinityManager.OnAffinityLevelChanged -= HandleAffinityLevelChanged;
        }

        public void BindCharacter(CharacterInstance character)
        {
            boundCharacter = character;
            RefreshProgress();
        }

        private void RebuildGiftList()
        {
            if (giftListContainer == null) return;

            for (int i = giftListContainer.childCount - 1; i >= 0; i--)
                Destroy(giftListContainer.GetChild(i).gameObject);

            foreach (var gift in giftCatalog)
            {
                if (gift == null || giftEntryPrefab == null) continue;

                var entry = Instantiate(giftEntryPrefab, giftListContainer);
                var button = entry.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => HandleGiftSelected(gift));
            }
        }

        private void HandleGiftSelected(GiftItemData gift)
        {
            if (boundCharacter == null || affinityManager == null) return;
            affinityManager.GiveGift(boundCharacter, gift);
            RefreshProgress();
        }

        private void HandleAffinityLevelChanged(CharacterInstance character, int level)
        {
            if (character == boundCharacter) RefreshProgress();
        }

        private void RefreshProgress()
        {
            if (boundCharacter == null) return;

            if (affinityLevelText != null) affinityLevelText.text = $"Lv. {boundCharacter.affinityLevel}";
            if (affinityProgressBar != null)
                affinityProgressBar.fillAmount = Mathf.Clamp01((boundCharacter.affinityPoints % AffinityManager.PointsPerLevel) / (float)AffinityManager.PointsPerLevel);
        }
    }
}
