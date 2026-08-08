using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // Generic in-memory cache for server responses, keyed by string, with a per-entry expiry so a
    // caller can ask "is this still fresh enough to skip a network call" without hand-rolling
    // Time.realtimeSinceStartup bookkeeping at every call site.
    public class CacheManager : MonoBehaviour, IService
    {
        private class CacheEntry
        {
            public object Value;
            public float ExpiresAtRealtime;
        }

        [SerializeField] private float defaultTtlSeconds = 60f;

        private readonly Dictionary<string, CacheEntry> entries = new();

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(CacheManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            entries.Clear();
            return Task.CompletedTask;
        }

        public void Set<T>(string key, T value, float? ttlSeconds = null)
        {
            if (string.IsNullOrEmpty(key)) return;

            entries[key] = new CacheEntry
            {
                Value = value,
                ExpiresAtRealtime = Time.realtimeSinceStartup + (ttlSeconds ?? defaultTtlSeconds)
            };
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default;
            if (string.IsNullOrEmpty(key) || !entries.TryGetValue(key, out var entry)) return false;

            if (Time.realtimeSinceStartup >= entry.ExpiresAtRealtime)
            {
                entries.Remove(key);
                return false;
            }

            if (entry.Value is T typed)
            {
                value = typed;
                return true;
            }

            return false;
        }

        public void Invalidate(string key)
        {
            if (key != null) entries.Remove(key);
        }

        public void Clear() => entries.Clear();

        // Returns the cached value if still fresh, otherwise awaits fetchAsync and caches its result.
        public async Task<T> GetOrFetchAsync<T>(string key, Func<Task<T>> fetchAsync, float? ttlSeconds = null)
        {
            if (TryGet<T>(key, out var cached)) return cached;

            T fetched = await fetchAsync();
            Set(key, fetched, ttlSeconds);
            return fetched;
        }
    }
}
