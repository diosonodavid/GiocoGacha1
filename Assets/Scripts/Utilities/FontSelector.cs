using System;
using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.Utilities
{
    [Serializable]
    public class LanguageFontEntry
    {
        public string languageCode;
        public Font font;
    }

    // Swaps every managed Text to a language-appropriate font when LocalizationManager fires
    // OnLanguageChanged (e.g. a CJK font for "ja"/"ko"/"zh" instead of the default Latin font,
    // which typically can't render those glyphs at all).
    public class FontSelector : MonoBehaviour
    {
        [SerializeField] private List<LanguageFontEntry> fontsByLanguage = new();
        [SerializeField] private Font fallbackFont;
        [SerializeField] private Text[] managedTexts = Array.Empty<Text>();

        private LocalizationManager localizationManager;

        private void OnEnable()
        {
            if (ServiceLocator.Instance.TryGet(out localizationManager))
            {
                localizationManager.OnLanguageChanged += HandleLanguageChanged;
                HandleLanguageChanged(localizationManager.CurrentLanguageCode);
            }
        }

        private void OnDisable()
        {
            if (localizationManager != null)
                localizationManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLanguageChanged(string languageCode)
        {
            Font font = GetFontFor(languageCode);
            if (font == null) return;

            foreach (var text in managedTexts)
            {
                if (text != null) text.font = font;
            }
        }

        private Font GetFontFor(string languageCode)
        {
            foreach (var entry in fontsByLanguage)
            {
                if (entry.languageCode == languageCode) return entry.font;
            }

            return fallbackFont;
        }
    }
}
