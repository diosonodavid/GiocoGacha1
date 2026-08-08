using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Utilities
{
    // JsonUtility can't serialize a bare top-level array/List<T> or a Dictionary at all; this
    // wraps both in the same "flatten to a serializable container" trick SerializableDictionary
    // already uses for the Inspector, but aimed at JSON payloads (e.g. NetworkManager responses,
    // CloudSaveService blobs) instead.
    public static class JsonDataConverter
    {
        [Serializable]
        private class ListWrapper<T>
        {
            public List<T> items = new();
        }

        [Serializable]
        private class DictionaryWrapper<TKey, TValue>
        {
            public List<TKey> keys = new();
            public List<TValue> values = new();
        }

        public static string ToJson<T>(T value, bool prettyPrint = false) => JsonUtility.ToJson(value, prettyPrint);

        public static T FromJson<T>(string json) => !string.IsNullOrEmpty(json) ? JsonUtility.FromJson<T>(json) : default;

        public static string ToJsonList<T>(List<T> list) => JsonUtility.ToJson(new ListWrapper<T> { items = list ?? new List<T>() });

        public static List<T> FromJsonList<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<T>();
            var wrapper = JsonUtility.FromJson<ListWrapper<T>>(json);
            return wrapper?.items ?? new List<T>();
        }

        public static string ToJsonDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var wrapper = new DictionaryWrapper<TKey, TValue>();
            if (dictionary != null)
            {
                foreach (var kvp in dictionary)
                {
                    wrapper.keys.Add(kvp.Key);
                    wrapper.values.Add(kvp.Value);
                }
            }

            return JsonUtility.ToJson(wrapper);
        }

        public static Dictionary<TKey, TValue> FromJsonDictionary<TKey, TValue>(string json)
        {
            var result = new Dictionary<TKey, TValue>();
            if (string.IsNullOrEmpty(json)) return result;

            var wrapper = JsonUtility.FromJson<DictionaryWrapper<TKey, TValue>>(json);
            if (wrapper == null) return result;

            int count = Mathf.Min(wrapper.keys.Count, wrapper.values.Count);
            for (int i = 0; i < count; i++)
                result[wrapper.keys[i]] = wrapper.values[i];

            return result;
        }
    }
}
