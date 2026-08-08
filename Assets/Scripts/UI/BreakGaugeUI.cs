using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Battle HUD widget (not a full UIController screen) reflecting a boss's BreakSystem gauge.
    public class BreakGaugeUI : MonoBehaviour
    {
        [SerializeField] private Image gaugeFillImage;
        [SerializeField] private GameObject vulnerableIndicator;

        public void Refresh(float gaugeRatio, bool isBroken)
        {
            if (gaugeFillImage != null) gaugeFillImage.fillAmount = Mathf.Clamp01(gaugeRatio);
            if (vulnerableIndicator != null) vulnerableIndicator.SetActive(isBroken);
        }
    }
}
