using System.Collections;
using UnityEngine;

namespace GachaGame.Utilities
{
    // Fades a full-screen blur overlay (a UI Image using a blur material/shader) in or out behind
    // menus/pop-ups, via CanvasGroup.alpha rather than toggling the shader itself per-frame.
    public class UIBlurController : MonoBehaviour
    {
        [SerializeField] private GameObject blurOverlay;
        [SerializeField] private float fadeDurationSeconds = 0.25f;

        private CanvasGroup overlayCanvasGroup;
        private Coroutine activeFade;

        private void Awake()
        {
            if (blurOverlay == null) return;
            overlayCanvasGroup = blurOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null) overlayCanvasGroup = blurOverlay.AddComponent<CanvasGroup>();
        }

        public void ShowBlur() => SetBlur(true);
        public void HideBlur() => SetBlur(false);

        private void SetBlur(bool visible)
        {
            if (blurOverlay == null) return;
            if (activeFade != null) StopCoroutine(activeFade);

            if (visible) blurOverlay.SetActive(true);
            activeFade = StartCoroutine(FadeRoutine(visible));
        }

        private IEnumerator FadeRoutine(bool visible)
        {
            float start = overlayCanvasGroup.alpha;
            float end = visible ? 1f : 0f;
            float elapsed = 0f;

            while (elapsed < fadeDurationSeconds)
            {
                elapsed += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(start, end, elapsed / fadeDurationSeconds);
                yield return null;
            }

            overlayCanvasGroup.alpha = end;
            if (!visible) blurOverlay.SetActive(false);
        }
    }
}
