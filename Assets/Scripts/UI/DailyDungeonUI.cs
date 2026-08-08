using System;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Lists today's open dungeons (DailyDungeonManager.GetDungeonsOpenToday), grouping entries
    // that share a DungeonType as difficulty options for the same dungeon rather than adding a
    // separate difficulty field to DailyDungeonData.
    public class DailyDungeonUI : UIController
    {
        [SerializeField] private Transform dungeonListContainer;
        [SerializeField] private GameObject dungeonEntryPrefab;

        private DailyDungeonManager dailyDungeonManager;

        public event Action<DailyDungeonData> OnDungeonSelected;

        protected override void OnShown()
        {
            if (!ServiceLocator.Instance.TryGet(out dailyDungeonManager))
            {
                Debug.LogWarning($"{nameof(DailyDungeonUI)} could not resolve {nameof(DailyDungeonManager)}.", this);
                return;
            }

            RefreshList();
        }

        public void RefreshList()
        {
            if (dailyDungeonManager == null || dungeonListContainer == null || dungeonEntryPrefab == null) return;

            for (int i = dungeonListContainer.childCount - 1; i >= 0; i--)
                Destroy(dungeonListContainer.GetChild(i).gameObject);

            foreach (var dungeon in dailyDungeonManager.GetDungeonsOpenToday())
                BuildEntry(dungeon);
        }

        private void BuildEntry(DailyDungeonData dungeon)
        {
            var go = Instantiate(dungeonEntryPrefab, dungeonListContainer);
            int remaining = dailyDungeonManager.GetAttemptsRemaining(dungeon.dungeonId);

            var label = go.GetComponentInChildren<Text>();
            if (label != null) label.text = $"{dungeon.dungeonName}  ({remaining} left today)";

            var button = go.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.interactable = remaining > 0;
                button.onClick.AddListener(() => OnDungeonSelected?.Invoke(dungeon));
            }
        }
    }
}
