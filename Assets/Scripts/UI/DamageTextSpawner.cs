using System.Collections;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Spawns floating damage numbers above a world-space anchor, pulled from an ObjectPooler
    // instead of Instantiate/Destroy to avoid per-hit GC allocations during battle.
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private GameObject criticalDamageTextPrefab;
        [SerializeField] private Vector3 spawnOffset = new(0f, 1.5f, 0f);
        [SerializeField] private float floatDistance = 1f;
        [SerializeField] private float lifetimeSeconds = 1f;
        [SerializeField] private ObjectPooler objectPooler;

        public void Spawn(Vector3 worldPosition, DamageResult result)
        {
            var prefab = result.isCritical && criticalDamageTextPrefab != null ? criticalDamageTextPrefab : damageTextPrefab;
            if (prefab == null) return;

            Vector3 spawnPosition = worldPosition + spawnOffset;
            GameObject instance = objectPooler != null
                ? objectPooler.Get(prefab, spawnPosition, Quaternion.identity)
                : Instantiate(prefab, spawnPosition, Quaternion.identity);

            var label = instance.GetComponentInChildren<Text>();
            if (label != null) label.text = result.damageAmount.ToString();

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
