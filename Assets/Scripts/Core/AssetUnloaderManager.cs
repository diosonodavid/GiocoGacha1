using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // Periodically (and on-demand, e.g. after a scene transition) frees resources/textures that are
    // no longer referenced from the loaded scene, via Resources.UnloadUnusedAssets - the standard
    // Unity mechanism for this rather than any custom tracking.
    public class AssetUnloaderManager : MonoBehaviour, IService
    {
        [SerializeField] private float autoUnloadIntervalSeconds = 120f;

        private float timer;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(AssetUnloaderManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < autoUnloadIntervalSeconds) return;

            timer = 0f;
            UnloadUnusedAssets();
        }

        public void UnloadUnusedAssets()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
    }
}
