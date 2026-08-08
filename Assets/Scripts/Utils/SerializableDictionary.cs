using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Utils
{
    // Unity cannot serialize System.Collections.Generic.Dictionary directly, so this wrapper
    // flattens it to parallel key/value lists for the inspector and JsonUtility, and rebuilds
    // the dictionary on load.
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new();
        [SerializeField] private List<TValue> values = new();

        private readonly Dictionary<TKey, TValue> dictionary = new();

        public Dictionary<TKey, TValue> Dictionary => dictionary;

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in dictionary)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            dictionary.Clear();
            int count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
                dictionary[keys[i]] = values[i];
        }
    }
}
