using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    [Serializable]
    public class BundleVersionEntry
    {
        public string bundleId;
        public string localVersion;
        public string remoteVersion;
    }

    // Tracks which version of each downloaded AssetBundle/patch is present locally versus what the
    // server currently offers, so PatchDownloader/AddressablesManager callers can tell "is this
    // bundle stale" without re-deriving it themselves each time.
    public class BundleCatalogManager : MonoBehaviour, IService
    {
        private readonly Dictionary<string, BundleVersionEntry> catalog = new();

        public IReadOnlyDictionary<string, BundleVersionEntry> Catalog => catalog;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(BundleCatalogManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            catalog.Clear();
            return Task.CompletedTask;
        }

        public void RegisterLocalVersion(string bundleId, string version) =>
            GetOrCreateEntry(bundleId).localVersion = version;

        public void RegisterRemoteVersion(string bundleId, string version) =>
            GetOrCreateEntry(bundleId).remoteVersion = version;

        public bool IsOutdated(string bundleId)
        {
            if (bundleId == null || !catalog.TryGetValue(bundleId, out var entry)) return false;
            return !string.IsNullOrEmpty(entry.remoteVersion) && entry.remoteVersion != entry.localVersion;
        }

        private BundleVersionEntry GetOrCreateEntry(string bundleId)
        {
            if (!catalog.TryGetValue(bundleId, out var entry))
            {
                entry = new BundleVersionEntry { bundleId = bundleId };
                catalog[bundleId] = entry;
            }

            return entry;
        }
    }
}
