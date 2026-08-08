using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class ProfileCustomizationUI : UIController
    {
        [SerializeField] private Transform titleListContainer;
        [SerializeField] private GameObject titleEntryPrefab;
        [SerializeField] private Transform emblemListContainer;
        [SerializeField] private GameObject emblemEntryPrefab;
        [SerializeField] private Text equippedTitleText;
        [SerializeField] private Text equippedEmblemText;

        private TitleHonorManager titleHonorManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out titleHonorManager);

            if (titleHonorManager != null)
            {
                titleHonorManager.OnEquippedTitleChanged += HandleTitleChanged;
                titleHonorManager.OnEquippedEmblemChanged += HandleEmblemChanged;
            }

            RebuildLists();
            RefreshEquippedDisplay();
        }

        protected override void OnHidden()
        {
            if (titleHonorManager != null)
            {
                titleHonorManager.OnEquippedTitleChanged -= HandleTitleChanged;
                titleHonorManager.OnEquippedEmblemChanged -= HandleEmblemChanged;
            }
        }

        public void HandleTitleSelected(string titleId) => titleHonorManager?.TryEquipTitle(titleId);

        public void HandleEmblemSelected(string emblemId) => titleHonorManager?.TryEquipEmblem(emblemId);

        private void HandleTitleChanged(string titleId) => RefreshEquippedDisplay();

        private void HandleEmblemChanged(string emblemId) => RefreshEquippedDisplay();

        private void RebuildLists()
        {
            ClearContainer(titleListContainer);
            ClearContainer(emblemListContainer);
            if (titleHonorManager == null) return;

            foreach (var titleId in titleHonorManager.UnlockedTitleIds)
                BuildEntry(titleListContainer, titleEntryPrefab, titleId);

            foreach (var emblemId in titleHonorManager.UnlockedEmblemIds)
                BuildEntry(emblemListContainer, emblemEntryPrefab, emblemId);
        }

        private void BuildEntry(Transform container, GameObject prefab, string id)
        {
            if (container == null || prefab == null) return;

            var entry = Instantiate(prefab, container);
            var label = entry.GetComponentInChildren<Text>();
            if (label != null) label.text = id;
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        private void RefreshEquippedDisplay()
        {
            if (equippedTitleText != null) equippedTitleText.text = titleHonorManager?.EquippedTitleId ?? string.Empty;
            if (equippedEmblemText != null) equippedEmblemText.text = titleHonorManager?.EquippedEmblemId ?? string.Empty;
        }
    }
}
