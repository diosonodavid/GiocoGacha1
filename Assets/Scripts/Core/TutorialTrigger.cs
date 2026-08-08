using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Core
{
    // Attach to a screen's root object; fires its tutorial sequence the first time that screen
    // becomes active, then never again (tracked via TutorialManager's persisted completed-step ids).
    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private List<TutorialStepData> sequence = new();

        private TutorialManager tutorialManager;

        private void OnEnable()
        {
            ServiceLocator.Instance.TryGet(out tutorialManager);
            TryTrigger();
        }

        public void TryTrigger()
        {
            if (tutorialManager == null || sequence.Count == 0) return;
            if (tutorialManager.IsTutorialActive) return;
            if (tutorialManager.IsStepCompleted(sequence[0].stepId)) return;

            tutorialManager.StartSequence(sequence);
        }
    }
}
