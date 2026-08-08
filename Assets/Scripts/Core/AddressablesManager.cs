using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GachaGame.Core
{
    // Named "AddressablesManager" per the design spec, but implemented over
    // UnityWebRequestAssetBundle rather than the Addressables package - Packages/manifest.json only
    // carries com.unity.modules.assetbundle/unitywebrequestassetbundle, not com.unity.addressables,
    // so this avoids depending on an uninstalled package (same check PostProcessingController made
    // for URP before using it). Assets are addressed by a bundle URL + in-bundle asset name pair
    // instead of an Addressable key, and downloaded bundles are cached until explicitly unloaded.
    public class AddressablesManager : MonoBehaviour, IService
    {
        private readonly Dictionary<string, AssetBundle> loadedBundles = new();

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(AddressablesManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            foreach (var bundle in loadedBundles.Values) bundle.Unload(false);
            loadedBundles.Clear();
            return Task.CompletedTask;
        }

        public async Task<T> LoadAssetAsync<T>(string bundleUrl, string assetName) where T : Object
        {
            var bundle = await GetOrDownloadBundleAsync(bundleUrl);
            if (bundle == null) return null;

            var request = bundle.LoadAssetAsync<T>(assetName);
            while (!request.isDone) await Task.Yield();

            return request.asset as T;
        }

        public void UnloadBundle(string bundleUrl, bool unloadAllLoadedObjects = false)
        {
            if (bundleUrl == null || !loadedBundles.TryGetValue(bundleUrl, out var bundle)) return;

            bundle.Unload(unloadAllLoadedObjects);
            loadedBundles.Remove(bundleUrl);
        }

        private async Task<AssetBundle> GetOrDownloadBundleAsync(string bundleUrl)
        {
            if (string.IsNullOrEmpty(bundleUrl)) return null;
            if (loadedBundles.TryGetValue(bundleUrl, out var cached)) return cached;

            using var request = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"Failed to download asset bundle {bundleUrl}: {request.error}");
                return null;
            }

            var bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle != null) loadedBundles[bundleUrl] = bundle;
            return bundle;
        }
    }
}
