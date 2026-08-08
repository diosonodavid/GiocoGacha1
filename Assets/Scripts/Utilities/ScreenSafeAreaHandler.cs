using UnityEngine;

namespace GachaGame.Utilities
{
    // Standard safe-area-to-anchors conversion so UI avoids notches/rounded corners on mobile;
    // re-checks every frame since rotating the device (see OrientationHandler) changes the safe
    // area at runtime, not just at startup.
    [RequireComponent(typeof(RectTransform))]
    public class ScreenSafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;

        private void Awake() => rectTransform = GetComponent<RectTransform>();

        private void OnEnable() => ApplySafeArea();

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea) ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
