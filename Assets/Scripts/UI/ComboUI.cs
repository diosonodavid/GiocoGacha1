using GachaGame.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Battle HUD widget (not a full UIController screen) reflecting ComboSystem's live count.
    public class ComboUI : MonoBehaviour
    {
        [SerializeField] private Text comboCountText;
        [SerializeField] private Animator comboAnimator;
        [SerializeField] private string comboPulseTrigger = "ComboPulse";

        private ComboSystem boundCombo;

        public void Bind(ComboSystem comboSystem)
        {
            if (boundCombo != null)
            {
                boundCombo.OnComboChanged -= HandleComboChanged;
                boundCombo.OnComboReset -= HandleComboReset;
            }

            boundCombo = comboSystem;

            if (boundCombo != null)
            {
                boundCombo.OnComboChanged += HandleComboChanged;
                boundCombo.OnComboReset += HandleComboReset;
            }
        }

        private void HandleComboChanged(int count)
        {
            if (comboCountText != null) comboCountText.text = $"x{count}";
            if (comboAnimator != null) comboAnimator.SetTrigger(comboPulseTrigger);
        }

        private void HandleComboReset()
        {
            if (comboCountText != null) comboCountText.text = string.Empty;
        }
    }
}
