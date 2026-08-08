using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Settings-menu language picker; lists the configured language codes and switches via the
    // existing LocalizationManager service rather than owning any language data itself.
    public class LanguageSelectorUI : UIController
    {
        [SerializeField] private List<string> availableLanguageCodes = new() { "en", "it" };
        [SerializeField] private Transform languageButtonContainer;
        [SerializeField] private GameObject languageButtonPrefab;

        private LocalizationManager localizationManager;

        protected override void OnShown()
        {
            if (!ServiceLocator.Instance.TryGet(out localizationManager))
            {
                Debug.LogWarning($"{nameof(LanguageSelectorUI)} could not resolve {nameof(LocalizationManager)}.", this);
                return;
            }

            BuildLanguageButtons();
        }

        private void BuildLanguageButtons()
        {
            if (languageButtonContainer == null || languageButtonPrefab == null) return;

            for (int i = languageButtonContainer.childCount - 1; i >= 0; i--)
                Destroy(languageButtonContainer.GetChild(i).gameObject);

            foreach (var code in availableLanguageCodes)
            {
                var go = Instantiate(languageButtonPrefab, languageButtonContainer);

                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = code.ToUpperInvariant();

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => _ = localizationManager.LoadLanguageAsync(code));
            }
        }
    }
}
