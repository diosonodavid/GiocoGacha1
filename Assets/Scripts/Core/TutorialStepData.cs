using UnityEngine;

namespace GachaGame.Core
{
    [CreateAssetMenu(fileName = "TutorialStepData", menuName = "GachaGame/Core/Tutorial Step Data")]
    public class TutorialStepData : ScriptableObject
    {
        public string stepId;
        public string targetUIElementId;
        [TextArea] public string dialogueText;
        public Rect highlightAreaRect;
        public bool actionRequired;
    }
}
