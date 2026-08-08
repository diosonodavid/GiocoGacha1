using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Visual representation of a single combatant during battle: name, HP bar, and a row of
    // buff/debuff icons refreshed from ICombatant.ActiveStatusEffects each turn.
    public class BattlePortraitWidget : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Slider hpBar;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Transform statusIconContainer;
        [SerializeField] private GameObject buffIconPrefab;
        [SerializeField] private GameObject debuffIconPrefab;

        public ICombatant BoundCombatant { get; private set; }

        public void Bind(ICombatant combatant, string displayName, Sprite portrait)
        {
            BoundCombatant = combatant;
            if (nameText != null) nameText.text = displayName;
            if (portraitImage != null) portraitImage.sprite = portrait;
            Refresh();
        }

        public void Refresh()
        {
            if (BoundCombatant == null) return;

            if (hpBar != null)
            {
                hpBar.maxValue = Mathf.Max(1, BoundCombatant.MaxHp);
                hpBar.value = BoundCombatant.CurrentHp;
            }

            RefreshStatusIcons();
        }

        private void RefreshStatusIcons()
        {
            if (statusIconContainer == null) return;

            for (int i = statusIconContainer.childCount - 1; i >= 0; i--)
                Destroy(statusIconContainer.GetChild(i).gameObject);

            foreach (var effect in BoundCombatant.ActiveStatusEffects)
            {
                bool isBuff = effect.value >= 0f;
                var prefab = isBuff ? buffIconPrefab : debuffIconPrefab;
                if (prefab != null) Instantiate(prefab, statusIconContainer);
            }
        }
    }
}
