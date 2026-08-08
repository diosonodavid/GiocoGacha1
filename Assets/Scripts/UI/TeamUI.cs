using GachaGame.Core;
using GachaGame.Inventory;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Team composition screen for the active TeamManager loadout. Implements the "quick select"
    // flow (tap an owned character, then tap a slot to place it there) rather than full
    // drag-and-drop, since it reaches the same result with far less input-handling code; a
    // drag-and-drop version would call TeamManager.TrySetSlot from an OnDrop handler instead.
    public class TeamUI : UIController
    {
        [SerializeField] private Transform characterListContainer;
        [SerializeField] private GameObject characterEntryPrefab;
        [SerializeField] private Transform[] slotContainers = new Transform[TeamData.MaxSlots];
        [SerializeField] private GameObject slotEntryPrefab;

        private TeamManager teamManager;
        private InventoryManager inventoryManager;
        private string selectedBaseDataId;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out teamManager);
            ServiceLocator.Instance.TryGet(out inventoryManager);

            if (teamManager != null) teamManager.OnTeamSlotChanged += HandleSlotChanged;

            RefreshCharacterList();
            RefreshAllSlots();
        }

        protected override void OnHidden()
        {
            if (teamManager != null) teamManager.OnTeamSlotChanged -= HandleSlotChanged;
        }

        public void SelectCharacter(string baseDataId) => selectedBaseDataId = baseDataId;

        public void AssignSelectedToSlot(int slotIndex)
        {
            if (teamManager == null || string.IsNullOrEmpty(selectedBaseDataId)) return;

            teamManager.TrySetSlot(teamManager.ActiveTeamIndex, slotIndex, selectedBaseDataId);
            selectedBaseDataId = null;
        }

        public void ClearSlot(int slotIndex) => teamManager?.ClearSlot(teamManager.ActiveTeamIndex, slotIndex);

        public void RefreshCharacterList()
        {
            if (inventoryManager == null || characterListContainer == null || characterEntryPrefab == null) return;

            ClearContainer(characterListContainer);

            foreach (var entry in inventoryManager.OwnedCharacters.Values)
            {
                var go = Instantiate(characterEntryPrefab, characterListContainer);
                var button = go.GetComponent<Button>();
                string baseDataId = entry.baseDataId;
                if (button != null) button.onClick.AddListener(() => SelectCharacter(baseDataId));
            }
        }

        public void RefreshAllSlots()
        {
            if (slotContainers == null) return;
            for (int i = 0; i < slotContainers.Length; i++)
                RefreshSlot(i);
        }

        private void HandleSlotChanged(int teamIndex, int slotIndex)
        {
            if (teamManager != null && teamIndex == teamManager.ActiveTeamIndex) RefreshSlot(slotIndex);
        }

        private void RefreshSlot(int slotIndex)
        {
            if (teamManager?.ActiveTeam == null) return;
            if (slotContainers == null || slotIndex >= slotContainers.Length) return;

            var container = slotContainers[slotIndex];
            if (container == null || slotEntryPrefab == null) return;

            ClearContainer(container);

            string baseDataId = teamManager.ActiveTeam.memberBaseDataIds[slotIndex];
            if (string.IsNullOrEmpty(baseDataId)) return;

            var go = Instantiate(slotEntryPrefab, container);
            var label = go.GetComponentInChildren<Text>();
            if (label != null) label.text = slotIndex == TeamData.LeaderSlotIndex ? $"{baseDataId} (Leader)" : baseDataId;

            var button = go.GetComponent<Button>();
            if (button != null) button.onClick.AddListener(() => ClearSlot(slotIndex));
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
