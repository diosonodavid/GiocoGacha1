using System;
using System.Collections;
using UnityEngine;

namespace GachaGame.Utilities
{
    // Drives a dissolve-shader float property via a MaterialPropertyBlock (so it doesn't create a
    // per-instance material), used for the defeated-enemy fade-out.
    public class DissolveEffectController : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private string dissolveAmountProperty = "_DissolveAmount";
        [SerializeField] private float dissolveDurationSeconds = 1f;

        private MaterialPropertyBlock propertyBlock;

        private void Awake() => propertyBlock = new MaterialPropertyBlock();

        public void PlayDissolve(Action onComplete = null) => StartCoroutine(DissolveRoutine(onComplete));

        private IEnumerator DissolveRoutine(Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < dissolveDurationSeconds)
            {
                elapsed += Time.deltaTime;
                ApplyDissolveAmount(Mathf.Clamp01(elapsed / dissolveDurationSeconds));
                yield return null;
            }

            ApplyDissolveAmount(1f);
            onComplete?.Invoke();
        }

        private void ApplyDissolveAmount(float amount)
        {
            if (targetRenderer == null) return;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(dissolveAmountProperty, amount);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
