using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Utils
{
    // Loads "Localization/{languageCode}" as a JSON-encoded key/value table (via ResourceManager)
    // and raises OnLanguageChanged so any live UI can re-pull its strings without a scene reload.
    public class LocalizationManager : MonoBehaviour, IService
    {
        [Serializable]
        private class LocalizationTable
        {
            public List<string> keys = new();
            public List<string> values = new();
        }

        public event Action<string> OnLanguageChanged;

        [SerializeField] private string currentLanguageCode = "en";
        [SerializeField] private string fallbackLanguageCode = "en";

        private readonly Dictionary<string, string> currentStrings = new();
        private ResourceManager resourceManager;

        public string CurrentLanguageCode => currentLanguageCode;

        public async Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out resourceManager);
            await LoadLanguageAsync(currentLanguageCode);
            Debug.Log($"{nameof(LocalizationManager)} initialized.");
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public async Task LoadLanguageAsync(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;

            var table = resourceManager != null
                ? await resourceManager.LoadJsonAsync<LocalizationTable>($"Localization/{languageCode}")
                : null;

            if (table == null && languageCode != fallbackLanguageCode)
            {
                await LoadLanguageAsync(fallbackLanguageCode);
                return;
            }

            currentStrings.Clear();
            if (table != null)
            {
                int count = Mathf.Min(table.keys.Count, table.values.Count);
                for (int i = 0; i < count; i++)
                    currentStrings[table.keys[i]] = table.values[i];
            }

            currentLanguageCode = languageCode;
            OnLanguageChanged?.Invoke(currentLanguageCode);
        }

        public string Get(string key)
        {
            if (key == null) return string.Empty;
            return currentStrings.TryGetValue(key, out var value) ? value : key;
        }

        public string Get(string key, params object[] formatArgs)
        {
            string template = Get(key);
            try
            {
                return formatArgs != null && formatArgs.Length > 0 ? string.Format(template, formatArgs) : template;
            }
            catch (FormatException)
            {
                return template;
            }
        }
    }
}
