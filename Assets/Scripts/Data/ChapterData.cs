using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "ChapterData", menuName = "GachaGame/Data/Chapter Data")]
    public class ChapterData : ScriptableObject
    {
        public string chapterId;
        public string chapterTitle;
        public Sprite backgroundBanner;
        public List<StageData> stages = new();
    }
}
