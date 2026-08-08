using GachaGame.Core;
using UnityEngine;

namespace GachaGame.UI
{
    // Darkens the whole screen except a rectangular cutout around the current tutorial step's
    // target element, using four blocker panels around the hole instead of a stencil shader - each
    // panel also blocks raycasts, so taps outside the highlighted area are ignored during the tutorial.
    public class TutorialMaskUI : MonoBehaviour
    {
        [SerializeField] private RectTransform topBlocker;
        [SerializeField] private RectTransform bottomBlocker;
        [SerializeField] private RectTransform leftBlocker;
        [SerializeField] private RectTransform rightBlocker;
        [SerializeField] private GameObject root;

        private TutorialManager tutorialManager;

        private void OnEnable()
        {
            ServiceLocator.Instance.TryGet(out tutorialManager);
            if (tutorialManager == null) return;

            tutorialManager.OnStepStarted += HandleStepStarted;
            tutorialManager.OnTutorialCompleted += HandleTutorialCompleted;

            if (root != null) root.SetActive(tutorialManager.IsTutorialActive);
            if (tutorialManager.CurrentStep != null) ApplyHighlight(tutorialManager.CurrentStep.highlightAreaRect);
        }

        private void OnDisable()
        {
            if (tutorialManager == null) return;
            tutorialManager.OnStepStarted -= HandleStepStarted;
            tutorialManager.OnTutorialCompleted -= HandleTutorialCompleted;
        }

        private void HandleStepStarted(TutorialStepData step)
        {
            if (root != null) root.SetActive(true);
            if (step != null) ApplyHighlight(step.highlightAreaRect);
        }

        private void HandleTutorialCompleted()
        {
            if (root != null) root.SetActive(false);
        }

        // areaInScreenSpace uses Screen-space pixel coordinates, origin bottom-left, matching Rect
        // conventions used elsewhere in Unity (e.g. Screen.safeArea).
        private void ApplyHighlight(Rect areaInScreenSpace)
        {
            SizeBlocker(topBlocker, 0f, areaInScreenSpace.yMax, Screen.width, Screen.height);
            SizeBlocker(bottomBlocker, 0f, 0f, Screen.width, areaInScreenSpace.yMin);
            SizeBlocker(leftBlocker, 0f, areaInScreenSpace.yMin, areaInScreenSpace.xMin, areaInScreenSpace.yMax);
            SizeBlocker(rightBlocker, areaInScreenSpace.xMax, areaInScreenSpace.yMin, Screen.width, areaInScreenSpace.yMax);
        }

        private static void SizeBlocker(RectTransform blocker, float xMin, float yMin, float xMax, float yMax)
        {
            if (blocker == null) return;

            float width = Mathf.Max(0f, xMax - xMin);
            float height = Mathf.Max(0f, yMax - yMin);

            blocker.gameObject.SetActive(width > 0f && height > 0f);
            blocker.position = new Vector3(xMin + width / 2f, yMin + height / 2f, 0f);
            blocker.sizeDelta = new Vector2(width, height);
        }
    }
}
