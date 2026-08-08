using GachaGame.Core;
using GachaGame.Network;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Progress bar for a single active PatchDownloader download: percentage, remaining size, and a
    // rolling MB/s speed estimate sampled from the bytes-downloaded delta between progress events.
    public class PatchDownloadUI : UIController
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text speedText;
        [SerializeField] private Text remainingSizeText;

        private const float SpeedSampleIntervalSeconds = 0.5f;

        private PatchDownloader patchDownloader;
        private PatchManifestEntry activePatch;
        private long lastSampledBytes;
        private float speedSampleTimer;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out patchDownloader);
            if (patchDownloader != null) patchDownloader.OnDownloadProgress += HandleProgress;
        }

        protected override void OnHidden()
        {
            if (patchDownloader != null) patchDownloader.OnDownloadProgress -= HandleProgress;
        }

        public async void BeginDownload(PatchManifestEntry patch)
        {
            if (patchDownloader == null || patch == null) return;

            activePatch = patch;
            lastSampledBytes = 0;
            speedSampleTimer = 0f;
            if (progressSlider != null) progressSlider.value = 0f;
            if (remainingSizeText != null) remainingSizeText.text = FormatBytes(patch.sizeBytes);

            await patchDownloader.DownloadPatchAsync(patch);
        }

        private void HandleProgress(string bundleId, float progress)
        {
            if (activePatch == null || bundleId != activePatch.bundleId) return;

            if (progressSlider != null) progressSlider.value = progress;

            long downloadedBytes = (long)(activePatch.sizeBytes * progress);
            if (remainingSizeText != null) remainingSizeText.text = FormatBytes(activePatch.sizeBytes - downloadedBytes);

            UpdateSpeed(downloadedBytes);
        }

        private void UpdateSpeed(long downloadedBytes)
        {
            speedSampleTimer += Time.deltaTime;
            if (speedSampleTimer < SpeedSampleIntervalSeconds) return;

            long deltaBytes = downloadedBytes - lastSampledBytes;
            float mbPerSecond = Mathf.Max(0f, (deltaBytes / 1024f / 1024f) / speedSampleTimer);
            if (speedText != null) speedText.text = $"{mbPerSecond:0.0} MB/s";

            lastSampledBytes = downloadedBytes;
            speedSampleTimer = 0f;
        }

        private static string FormatBytes(long bytes) => $"{bytes / 1024f / 1024f:0.0} MB";
    }
}
