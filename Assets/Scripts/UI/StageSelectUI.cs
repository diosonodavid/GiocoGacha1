using System;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Lists every chapter and its stages, greying out stages the player hasn't reached yet
    // (per StageManager.IsStageUnlocked) and surfacing the earned star count on cleared ones.
    public class StageSelectUI : UIController
    {
        [SerializeField] private Transform chapterListContainer;
        [SerializeField] private GameObject chapterEntryPrefab;
        [SerializeField] private GameObject stageEntryPrefab;

        private StageManager stageManager;

        public event Action<StageData> OnStageSelected;

        protected override void OnShown()
        {
            if (!ServiceLocator.Instance.TryGet(out stageManager))
            {
                Debug.LogWarning($"{nameof(StageSelectUI)} could not resolve {nameof(StageManager)}.", this);
                return;
            }

            RefreshChapterList();
        }

        public void RefreshChapterList()
        {
            if (stageManager == null || chapterListContainer == null || chapterEntryPrefab == null) return;

            ClearContainer(chapterListContainer);

            foreach (var chapter in stageManager.Chapters)
                BuildChapterEntry(chapter);
        }

        private void BuildChapterEntry(ChapterData chapter)
        {
            var chapterGo = Instantiate(chapterEntryPrefab, chapterListContainer);

            var titleText = chapterGo.GetComponentInChildren<Text>();
            if (titleText != null) titleText.text = chapter.chapterTitle;

            var banner = chapterGo.GetComponentInChildren<Image>();
            if (banner != null && chapter.backgroundBanner != null) banner.sprite = chapter.backgroundBanner;

            if (stageEntryPrefab == null) return;

            foreach (var stage in chapter.stages)
                BuildStageEntry(chapterGo.transform, stage);
        }

        private void BuildStageEntry(Transform container, StageData stage)
        {
            var stageGo = Instantiate(stageEntryPrefab, container);

            bool unlocked = stageManager.IsStageUnlocked(stage.stageId);
            int stars = stageManager.GetStars(stage.stageId);

            var label = stageGo.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = unlocked
                    ? $"{stage.stageName}  {stars}/{GameConstants.MaxStarsPerStage}"
                    : $"{stage.stageName}  (Locked)";
            }

            var button = stageGo.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.interactable = unlocked;
                button.onClick.AddListener(() => SelectStage(stage));
            }
        }

        public void SelectStage(StageData stage)
        {
            if (stage == null || stageManager == null || !stageManager.IsStageUnlocked(stage.stageId)) return;
            OnStageSelected?.Invoke(stage);
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
