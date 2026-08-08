using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GachaGame.Utilities
{
    // Toggles overrides on a single URP Volume's profile (Bloom/Vignette/MotionBlur) rather than
    // swapping whole VolumeProfiles, so quality-tier and battle-state changes both animate on the
    // same Volume instead of fighting each other.
    public class PostProcessingController : MonoBehaviour
    {
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private bool motionBlurDuringBattleOnly = true;

        private Bloom bloom;
        private Vignette vignette;
        private MotionBlur motionBlur;

        private void Awake()
        {
            if (postProcessVolume == null || postProcessVolume.profile == null) return;

            postProcessVolume.profile.TryGet(out bloom);
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out motionBlur);
        }

        // Called with the active QualitySettings level (e.g. from DevicePerformanceScaler) to
        // disable the heavier effects on lower-end tiers.
        public void ApplyQualityTier(int qualityLevel)
        {
            bool highQuality = qualityLevel >= QualitySettings.names.Length - 1;
            if (bloom != null) bloom.active = highQuality;
            if (vignette != null) vignette.active = highQuality;
        }

        public void SetBattleState(bool inBattle)
        {
            if (motionBlur != null && motionBlurDuringBattleOnly)
                motionBlur.active = inBattle;
        }
    }
}
