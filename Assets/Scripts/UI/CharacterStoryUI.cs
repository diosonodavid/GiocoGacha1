using System.Collections.Generic;
using GachaGame.Data;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class CharacterStoryUI : UIController
    {
        [SerializeField] private Transform chapterListContainer;
        [SerializeField] private GameObject chapterEntryPrefab;
        [SerializeField] private Text chapterTitleText;
        [SerializeField] private Text chapterBodyText;
        [SerializeField] private Button claimRewardButton;

        private readonly HashSet<string> claimedChapterIds = new();

        private CurrencyManager currencyManager;
        private CharacterInstance boundCharacter;
        private CharacterStoryData boundStory;
        private StoryChapter selectedChapter;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out currencyManager);
            if (claimRewardButton != null) claimRewardButton.onClick.AddListener(HandleClaimPressed);
        }

        protected override void OnHidden()
        {
            if (claimRewardButton != null) claimRewardButton.onClick.RemoveListener(HandleClaimPressed);
        }

        public void Bind(CharacterInstance character, CharacterStoryData story)
        {
            boundCharacter = character;
            boundStory = story;
            selectedChapter = null;
            RebuildChapterList();
        }

        private void RebuildChapterList()
        {
            if (chapterListContainer == null || boundStory == null) return;

            for (int i = chapterListContainer.childCount - 1; i >= 0; i--)
                Destroy(chapterListContainer.GetChild(i).gameObject);

            foreach (var chapter in boundStory.chapters)
            {
                if (chapter == null || chapterEntryPrefab == null) continue;

                bool unlocked = boundCharacter != null && boundCharacter.affinityLevel >= chapter.requiredAffinityLevel;

                var entry = Instantiate(chapterEntryPrefab, chapterListContainer);
                var label = entry.GetComponentInChildren<Text>();
                if (label != null) label.text = chapter.chapterTitle;

                var button = entry.GetComponentInChildren<Button>();
                if (button != null)
                {
                    button.interactable = unlocked;
                    button.onClick.AddListener(() => SelectChapter(chapter));
                }
            }
        }

        private void SelectChapter(StoryChapter chapter)
        {
            selectedChapter = chapter;

            if (chapterTitleText != null) chapterTitleText.text = chapter.chapterTitle;
            if (chapterBodyText != null)
                chapterBodyText.text = string.Join("\n\n", chapter.dialogueLines.ConvertAll(line => line != null ? line.text : string.Empty));
        }

        private void HandleClaimPressed()
        {
            if (selectedChapter == null || currencyManager == null) return;
            if (!claimedChapterIds.Add(selectedChapter.chapterId)) return;

            currencyManager.AddCurrency(CurrencyType.Gems, selectedChapter.gemRewardOnComplete);
        }
    }
}
