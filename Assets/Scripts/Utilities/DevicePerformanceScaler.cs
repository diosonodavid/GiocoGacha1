using UnityEngine;

namespace GachaGame.Utilities
{
    // Buckets the device into a quality tier from system RAM (a cheap proxy that doesn't require a
    // GPU benchmark) and applies it once at startup via QualitySettings, so lower-end devices
    // default to a playable frame rate instead of whatever quality level the project ships with.
    public class DevicePerformanceScaler : MonoBehaviour
    {
        [SerializeField] private int lowRamThresholdMb = 3000;
        [SerializeField] private int highRamThresholdMb = 6000;
        [SerializeField] private int lowEndTargetFrameRate = 30;
        [SerializeField] private int highEndTargetFrameRate = 60;

        private void Awake()
        {
            int qualityLevel = DetermineQualityLevel();
            QualitySettings.SetQualityLevel(qualityLevel, applyExpensiveChanges: true);
            Application.targetFrameRate = qualityLevel == 0 ? lowEndTargetFrameRate : highEndTargetFrameRate;
        }

        private int DetermineQualityLevel()
        {
            int ramMb = SystemInfo.systemMemorySize;
            int maxLevel = QualitySettings.names.Length - 1;

            if (ramMb <= lowRamThresholdMb) return 0;
            if (ramMb >= highRamThresholdMb) return maxLevel;
            return Mathf.Clamp(maxLevel / 2, 0, maxLevel);
        }
    }
}
