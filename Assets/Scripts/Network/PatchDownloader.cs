using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace GachaGame.Network
{
    [Serializable]
    public class PatchManifestEntry
    {
        public string bundleId;
        public string bundleUrl;
        public long sizeBytes;
        public string version;
    }

    [Serializable]
    public class PatchManifestResponse
    {
        public List<PatchManifestEntry> patches = new();
    }

    // Checks the server's patch manifest for DLC/AssetBundle updates newer than the client's local
    // version, then downloads them via UnityWebRequestAssetBundle - the same lower-level API
    // AddressablesManager builds on, so a downloaded patch bundle is directly loadable through it.
    public class PatchDownloader : MonoBehaviour, IService
    {
        public event Action<string, float> OnDownloadProgress; // bundleId, 0-1
        public event Action<string> OnDownloadComplete;

        private NetworkManager networkManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task<List<PatchManifestEntry>> CheckForPatchesAsync(string currentBundleVersion)
        {
            if (networkManager == null) return new List<PatchManifestEntry>();

            var response = await networkManager.GetAsync<PatchManifestResponse>($"/patches/manifest?clientVersion={currentBundleVersion}");
            return response.success && response.data != null ? response.data.patches : new List<PatchManifestEntry>();
        }

        public async Task<bool> DownloadPatchAsync(PatchManifestEntry patch)
        {
            if (patch == null || string.IsNullOrEmpty(patch.bundleUrl)) return false;

            using var request = UnityWebRequestAssetBundle.GetAssetBundle(patch.bundleUrl);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                OnDownloadProgress?.Invoke(patch.bundleId, operation.progress);
                await Task.Yield();
            }

            bool success = request.result == UnityWebRequest.Result.Success;
            if (success)
            {
                OnDownloadProgress?.Invoke(patch.bundleId, 1f);
                OnDownloadComplete?.Invoke(patch.bundleId);
            }

            return success;
        }
    }
}
