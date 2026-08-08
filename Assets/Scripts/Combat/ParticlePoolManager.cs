using System.Collections;
using GachaGame.Utils;
using UnityEngine;

namespace GachaGame.Combat
{
    // Thin wrapper over ObjectPooler specifically for impact/spell VFX, auto-releasing each
    // instance once its ParticleSystem finishes - mirrors DamageTextFX's coroutine-based release
    // over the same pooler.
    public class ParticlePoolManager : MonoBehaviour
    {
        [SerializeField] private ObjectPooler objectPooler;
        [SerializeField] private float fallbackLifetimeSeconds = 2f;

        public GameObject SpawnEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation)
        {
            if (effectPrefab == null) return null;

            GameObject instance = objectPooler != null
                ? objectPooler.Get(effectPrefab, position, rotation)
                : Instantiate(effectPrefab, position, rotation);

            StartCoroutine(ReleaseAfterLifetime(instance));
            return instance;
        }

        private IEnumerator ReleaseAfterLifetime(GameObject instance)
        {
            float lifetime = fallbackLifetimeSeconds;
            var particles = instance != null ? instance.GetComponent<ParticleSystem>() : null;
            if (particles != null) lifetime = particles.main.duration + particles.main.startLifetime.constantMax;

            yield return new WaitForSeconds(lifetime);

            if (objectPooler != null) objectPooler.Release(instance);
            else if (instance != null) Destroy(instance);
        }
    }
}
