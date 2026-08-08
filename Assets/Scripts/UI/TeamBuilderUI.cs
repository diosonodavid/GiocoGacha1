using System;
using System.Collections.Generic;
using System.Linq;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public enum TeamBuilderSortMode { Rarity, Element }

    // Lets the player pick from owned characters (sorted by rarity or element) and assign them
    // into a fixed-size team roster; TeamSlots is the source of truth consumed when entering battle.
    public class TeamBuilderUI : MonoBehaviour
    {
        private const int MaxTeamSize = 4;

        [SerializeField] private Transform characterListContainer;
        [SerializeField] private GameObject characterEntryPrefab;
        [SerializeField] private Transform[] teamSlotContainers = new Transform[MaxTeamSize];
        [SerializeField] private GameObject slotEntryPrefab;

        private InventoryManager inventoryManager;
        private TeamBuilderSortMode sortMode = TeamBuilderSortMode.Rarity;
        private readonly string[] teamSlots = new string[MaxTeamSize]; // baseDataId per slot, null = empty

        public IReadOnlyList<string> TeamSlots => teamSlots;

        private void OnEnable()
        {
            if (!ServiceLocator.Instance.TryGet(out inventoryManager))
            {
                Debug.LogWarning($"{nameof(TeamBuilderUI)} could not resolve {nameof(InventoryManager)}.", this);
                return;
            }
            RefreshCharacterList();
        }

        public void SetSortMode(TeamBuilderSortMode mode)
        {
            sortMode = mode;
            RefreshCharacterList();
        }

        public void RefreshCharacterList()
        {
            if (inventoryManager == null || characterListContainer == null || characterEntryPrefab == null) return;

            ClearContainer(characterListContainer);

            foreach (var entry in GetSortedOwnedCharacters())
            {
                var go = Instantiate(characterEntryPrefab, characterListContainer);
                var button = go.GetComponent<Button>();
                string baseDataId = entry.baseDataId;
                if (button != null) button.onClick.AddListener(() => TryAssignToFirstEmptySlot(baseDataId));
            }
        }

        // Owned characters ordered by the active sort mode. InventoryManager only tracks
        // baseDataId + memoryLevel (not the CharacterBaseData asset itself), so a true rarity/
        // element sort needs the caller's character database wired through; until then this
        // falls back to memory level (a reasonable proxy for rarity) or the raw id.
        private IEnumerable<CharacterInstance> GetSortedOwnedCharacters()
        {
            var characters = inventoryManager.OwnedCharacters.Values.AsEnumerable();
            return sortMode == TeamBuilderSortMode.Rarity
                ? characters.OrderByDescending(c => c.memoryLevel).ThenBy(c => c.baseDataId)
                : characters.OrderBy(c => c.baseDataId);
        }

        public bool TryAssignToSlot(int slotIndex, string baseDataId)
        {
            if (slotIndex < 0 || slotIndex >= MaxTeamSize) return false;
            if (baseDataId != null && Array.IndexOf(teamSlots, baseDataId) >= 0) return false;

            teamSlots[slotIndex] = baseDataId;
            RefreshSlotVisual(slotIndex);
            return true;
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxTeamSize) return;
            teamSlots[slotIndex] = null;
            RefreshSlotVisual(slotIndex);
        }

        public bool IsTeamComplete() => teamSlots.All(id => !string.IsNullOrEmpty(id));

        private void TryAssignToFirstEmptySlot(string baseDataId)
        {
            for (int i = 0; i < teamSlots.Length; i++)
            {
                if (string.IsNullOrEmpty(teamSlots[i]))
                {
                    TryAssignToSlot(i, baseDataId);
                    return;
                }
            }
        }

        private void RefreshSlotVisual(int slotIndex)
        {
            if (teamSlotContainers == null || slotIndex >= teamSlotContainers.Length) return;
            var container = teamSlotContainers[slotIndex];
            if (container == null || slotEntryPrefab == null) return;

            ClearContainer(container);
            if (string.IsNullOrEmpty(teamSlots[slotIndex])) return;

            var go = Instantiate(slotEntryPrefab, container);
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
