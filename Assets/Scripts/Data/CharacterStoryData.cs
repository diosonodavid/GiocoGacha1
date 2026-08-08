using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class StoryChapter
    {
        public string chapterId;
        public string chapterTitle;
        public int requiredAffinityLevel;
        public List<DialogueData> dialogueLines = new();
        public int gemRewardOnComplete;
    }

    [CreateAssetMenu(fileName = "CharacterStoryData", menuName = "GachaGame/Data/Character Story Data")]
    public class CharacterStoryData : ScriptableObject
    {
        public string storyId;
        public string characterId;
        public List<StoryChapter> chapters = new();
    }
}
