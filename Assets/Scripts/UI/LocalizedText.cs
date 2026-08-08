using GachaGame.Core;
using GachaGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Attach next to a Text component; keeps it in sync with the active language by re-pulling
    // LocalizationManager.Get(key) whenever OnLanguageChanged fires, instead of every screen
    // having to do that binding itself.
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string localizationKey;

        private Text targetText;
        private LocalizationManager localizationManager;

        private void Awake() => targetText = GetComponent<Text>();

        private void OnEnable()
        {
            if (ServiceLocator.Instance.TryGet(out localizationManager))
                localizationManager.OnLanguageChanged += HandleLanguageChanged;

            RefreshText();
        }

        private void OnDisable()
        {
            if (localizationManager != null)
                localizationManager.OnLanguageChanged -= HandleLanguageChanged;
        }

        public void SetKey(string key)
        {
            localizationKey = key;
            RefreshText();
        }

        private void HandleLanguageChanged(string languageCode) => RefreshText();

        private void RefreshText()
        {
            if (targetText == null || localizationManager == null || string.IsNullOrEmpty(localizationKey)) return;
            targetText.text = localizationManager.Get(localizationKey);
        }
    }
}
