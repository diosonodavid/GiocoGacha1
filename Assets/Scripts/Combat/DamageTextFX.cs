using System.Collections;
using GachaGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.Combat
{
    // Floats a damage or heal number above a world-space anchor, pooled via ObjectPooler like
    // UI.DamageTextSpawner. Kept separate from DamageTextSpawner because it also needs a "heal"
    // (positive number) path that plain DamageResult-based damage resolution doesn't have.
    public class DamageTextFX : MonoBehaviour
    {
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private GameObject criticalDamageTextPrefab;
        [SerializeField] private GameObject healTextPrefab;
        [SerializeField] private Vector3 spawnOffset = new(0f, 1.5f, 0f);
        [SerializeField] private float floatDistance = 1f;
        [SerializeField] private float lifetimeSeconds = 1f;
        [SerializeField] private ObjectPooler objectPooler;

        public void SpawnDamage(Vector3 worldPosition, int amount, bool isCritical)
        {
            var prefab = isCritical && criticalDamageTextPrefab != null ? criticalDamageTextPrefab : damageTextPrefab;
            Spawn(prefab, worldPosition, $"-{amount}");
        }

        public void SpawnHeal(Vector3 worldPosition, int amount)
        {
            var prefab = healTextPrefab != null ? healTextPrefab : damageTextPrefab;
            Spawn(prefab, worldPosition, $"+{amount}");
        }

        private void Spawn(GameObject prefab, Vector3 worldPosition, string text)
        {
            if (prefab == null) return;

            Vector3 spawnPosition = worldPosition + spawnOffset;
            GameObject instance = objectPooler != null
                ? objectPooler.Get(prefab, spawnPosition, Quaternion.identity)
                : Instantiate(prefab, spawnPosition, Quaternion.identity);

            var label = instance.GetComponentInChildren<Text>();
            if (label != null) label.text = text;

            StartCoroutine(AnimateAndRelease(instance, spawnPosition));
        }

        private IEnumerator AnimateAndRelease(GameObject instance, Vector3 startPosition)
        {
            float elapsed = 0f;
            Vector3 endPosition = startPosition + Vector3.up * floatDistance;

            while (elapsed < lifetimeSeconds)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetimeSeconds;
                instance.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }

            if (objectPooler != null) objectPooler.Release(instance);
            else Destroy(instance);
        }
    }
}
