using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Tower floor picker: lists floors up to TowerManager.MaxFloorReached + 1 (the next
    // unattempted floor) and previews the selected floor's stamina cost and reward items.
    public class TowerUI : UIController
    {
        [SerializeField] private List<TowerFloorData> floors = new();
        [SerializeField] private Transform floorListContainer;
        [SerializeField] private GameObject floorEntryPrefab;
        [SerializeField] private Text selectedFloorText;
        [SerializeField] private Text staminaCostText;
        [SerializeField] private Transform rewardPreviewContainer;
        [SerializeField] private GameObject rewardEntryPrefab;

        private TowerManager towerManager;
        private TowerFloorData selectedFloor;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out towerManager);
            RefreshFloorList();
        }

        public void RefreshFloorList()
        {
            if (floorListContainer == null || floorEntryPrefab == null) return;

            for (int i = floorListContainer.childCount - 1; i >= 0; i--)
                Destroy(floorListContainer.GetChild(i).gameObject);

            int highestSelectable = (towerManager?.MaxFloorReached ?? 0) + 1;

            foreach (var floor in floors)
            {
                var go = Instantiate(floorEntryPrefab, floorListContainer);

                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"Floor {floor.floorNumber}";

                var button = go.GetComponentInChildren<Button>();
                if (button != null)
                {
                    button.interactable = floor.floorNumber <= highestSelectable;
                    button.onClick.AddListener(() => SelectFloor(floor));
                }
            }
        }

        public void SelectFloor(TowerFloorData floor)
        {
            selectedFloor = floor;
            RefreshSelectedFloorPreview();
        }

        private void RefreshSelectedFloorPreview()
        {
            if (selectedFloor == null) return;

            if (selectedFloorText != null) selectedFloorText.text = $"Floor {selectedFloor.floorNumber}";
            if (staminaCostText != null) staminaCostText.text = selectedFloor.staminaCost.ToString();

            RefreshRewardPreview();
        }

        private void RefreshRewardPreview()
        {
            if (rewardPreviewContainer == null || rewardEntryPrefab == null) return;

            for (int i = rewardPreviewContainer.childCount - 1; i >= 0; i--)
                Destroy(rewardPreviewContainer.GetChild(i).gameObject);

            foreach (var item in selectedFloor.rewardItems)
            {
                var go = Instantiate(rewardEntryPrefab, rewardPreviewContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = item != null ? item.itemName : string.Empty;
            }
        }
    }
}
