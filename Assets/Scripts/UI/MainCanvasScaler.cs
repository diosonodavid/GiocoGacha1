using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Leans the CanvasScaler's width/height match toward whichever axis better fits the current
    // aspect ratio, instead of a single fixed match value baked in the inspector - keeps UI
    // elements from over-stretching on unusually narrow or wide device screens (tablets,
    // ultra-tall phones).
    [RequireComponent(typeof(CanvasScaler))]
    public class MainCanvasScaler : MonoBehaviour
    {
        [SerializeField] private float referenceAspect = 9f / 16f; // portrait reference (width / height)
        [SerializeField] private Vector2 referenceResolution = new(1080, 1920);

        private CanvasScaler canvasScaler;

        private void Awake()
        {
            canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = referenceResolution;
            ApplyScaling();
        }

        public void ApplyScaling()
        {
            float currentAspect = (float)Screen.width / Screen.height;

            // Match height when the device is narrower/taller than the reference (more vertical
            // space relative to width), match width otherwise - avoids letterboxing on both very
            // tall phones and wide tablets.
            canvasScaler.matchWidthOrHeight = currentAspect <= referenceAspect ? 1f : 0f;
        }
    }
}
