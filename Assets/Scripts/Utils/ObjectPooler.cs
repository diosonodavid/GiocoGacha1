using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Utils
{
    // Generic prefab pool keyed by the source prefab instance, so VFX/projectiles/damage-text can
    // share one pooler without pre-registering every prefab type up front.
    public class ObjectPooler : MonoBehaviour
    {
        [SerializeField] private int defaultCapacityPerPrefab = 16;

        private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();
        private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            if (!pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>(defaultCapacityPerPrefab);
                pools[prefab] = queue;
            }

            GameObject instance = queue.Count > 0 ? queue.Dequeue() : Instantiate(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            instanceToPrefab[instance] = prefab;
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            instance.SetActive(false);

            if (instanceToPrefab.TryGetValue(instance, out var prefab) && pools.TryGetValue(prefab, out var queue))
                queue.Enqueue(instance);
            else
                Destroy(instance);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;

            if (!pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>(count);
                pools[prefab] = queue;
            }

            for (int i = 0; i < count; i++)
            {
                var instance = Instantiate(prefab);
                instance.SetActive(false);
                instanceToPrefab[instance] = prefab;
                queue.Enqueue(instance);
            }
        }
    }
}
