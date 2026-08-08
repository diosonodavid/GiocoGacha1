using System.Collections;
using UnityEngine;

namespace GachaGame.Combat
{
    // Offsets the target transform by a random point inside a shrinking radius each frame, then
    // restores the original local position when done. Call from wherever a combat result is
    // resolved (e.g. a critical hit or a powerful skill landing).
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float criticalHitDuration = 0.2f;
        [SerializeField] private float criticalHitMagnitude = 0.15f;
        [SerializeField] private float powerfulSkillDuration = 0.35f;
        [SerializeField] private float powerfulSkillMagnitude = 0.3f;

        private Coroutine activeShake;
        private Vector3 originalLocalPosition;

        private void Awake()
        {
            if (cameraTransform == null) cameraTransform = transform;
            originalLocalPosition = cameraTransform.localPosition;
        }

        public void ShakeForCriticalHit() => Shake(criticalHitDuration, criticalHitMagnitude);

        public void ShakeForPowerfulSkill() => Shake(powerfulSkillDuration, powerfulSkillMagnitude);

        public void Shake(float duration, float magnitude)
        {
            if (cameraTransform == null || duration <= 0f) return;

            if (activeShake != null) StopCoroutine(activeShake);
            activeShake = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float damping = 1f - elapsed / duration;
                Vector2 offset = Random.insideUnitCircle * magnitude * damping;
                cameraTransform.localPosition = originalLocalPosition + new Vector3(offset.x, offset.y, 0f);
                yield return null;
            }

            cameraTransform.localPosition = originalLocalPosition;
            activeShake = null;
        }
    }
}
