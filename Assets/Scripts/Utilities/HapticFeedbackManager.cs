using UnityEngine;

namespace GachaGame.Utilities
{
    // Triggers device vibration on critical hits and UI taps via Handheld.Vibrate - the only haptic
    // API available without adding a mobile-haptics package, so intensity/pattern control isn't
    // available here (Android/iOS both just get a single fixed pulse).
    public class HapticFeedbackManager : MonoBehaviour
    {
        [SerializeField] private bool hapticsEnabled = true;

        public void SetHapticsEnabled(bool enabled) => hapticsEnabled = enabled;

        public void TriggerCriticalHitFeedback() => Vibrate();

        public void TriggerUITapFeedback() => Vibrate();

        private void Vibrate()
        {
            if (!hapticsEnabled) return;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
