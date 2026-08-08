using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class SkinWardrobeUI : UIController
    {
        [SerializeField] private Transform skinListContainer;
        [SerializeField] private GameObject skinEntryPrefab;
        [SerializeField] private Image previewImage;
        [SerializeField] private Button equipButton;
        [SerializeField] private List<SkinData> skinCatalog = new();

        private SkinManager skinManager;
        private CharacterInstance targetInstance;
        private SkinData selectedSkin;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out skinManager);
            if (equipButton != null) equipButton.onClick.AddListener(HandleEquipPressed);
        }

        protected override void OnHidden()
        {
            if (equipButton != null) equipButton.onClick.RemoveListener(HandleEquipPressed);
        }

        public void BindCharacter(CharacterInstance instance)
        {
            targetInstance = instance;
            selectedSkin = null;
            RebuildList();
        }

        private void RebuildList()
        {
            if (skinListContainer == null || skinEntryPrefab == null || targetInstance == null) return;

            for (int i = skinListContainer.childCount - 1; i >= 0; i--)
                Destroy(skinListContainer.GetChild(i).gameObject);

            foreach (var skin in skinCatalog)
            {
                if (skin == null || skin.characterId != targetInstance.baseDataId) continue;

                var go = Instantiate(skinEntryPrefab, skinListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = skin.skinName;

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => SelectSkin(skin));
            }
        }

        private void SelectSkin(SkinData skin)
        {
            selectedSkin = skin;
            if (previewImage != null) previewImage.sprite = skin.portraitSprite;
        }

        private void HandleEquipPressed()
        {
            if (skinManager == null || targetInstance == null || selectedSkin == null) return;
            skinManager.TryEquipSkin(targetInstance, selectedSkin);
        }
    }
}
