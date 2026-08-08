using GachaGame.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Playback transport bar for a bound BattleReplayManager: play/pause, adjustable fast-forward
    // speed (x2/x4 via SetSpeedMultiplier), and rewind-to-start. Drives
    // BattleReplayManager.TryConsumeNextCommand every frame while shown.
    public class BattleReplayUI : UIController
    {
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text speedLabelText;

        private BattleReplayManager replayManager;
        private float elapsedPlaybackSeconds;
        private float speedMultiplier = 1f;

        public void Bind(BattleReplayManager manager)
        {
            replayManager = manager;
            elapsedPlaybackSeconds = 0f;
            speedMultiplier = 1f;
            RefreshSpeedLabel();
        }

        protected override void OnHidden() => replayManager?.Pause();

        private void Update()
        {
            if (!IsShown || replayManager == null || !replayManager.IsPlaying) return;

            elapsedPlaybackSeconds += Time.deltaTime * speedMultiplier;
            while (replayManager.TryConsumeNextCommand(elapsedPlaybackSeconds, out _)) { }

            RefreshProgress();
        }

        public void HandlePlayPressed() => replayManager?.Play();
        public void HandlePausePressed() => replayManager?.Pause();

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Max(0.25f, multiplier);
            RefreshSpeedLabel();
        }

        public void Rewind()
        {
            if (replayManager?.ActiveReplay == null) return;

            elapsedPlaybackSeconds = 0f;
            replayManager.LoadReplay(replayManager.ActiveReplay);
            RefreshProgress();
        }

        private void RefreshProgress()
        {
            if (progressSlider == null || replayManager?.ActiveReplay == null) return;

            var commands = replayManager.ActiveReplay.commands;
            if (commands.Count == 0) return;

            float lastTimestamp = Mathf.Max(1f, commands[commands.Count - 1].timestampSeconds);
            progressSlider.value = Mathf.Clamp01(elapsedPlaybackSeconds / lastTimestamp);
        }

        private void RefreshSpeedLabel()
        {
            if (speedLabelText != null) speedLabelText.text = $"x{speedMultiplier:0.##}";
        }
    }
}
