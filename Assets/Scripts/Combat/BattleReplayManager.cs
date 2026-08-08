using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Combat
{
    // Records the command stream during a live battle and replays it back on demand. Plain class,
    // like BattleTurnController - ephemeral per-battle/per-viewing state, not a persistent service.
    public class BattleReplayManager
    {
        public event Action<BattleReplayCommand> OnCommandPlayed;
        public event Action OnPlaybackFinished;

        private int playbackIndex;

        public BattleReplayData ActiveReplay { get; private set; }
        public bool IsRecording { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        public void StartRecording(int randomSeed, string battleId)
        {
            ActiveReplay = new BattleReplayData
            {
                replayId = Guid.NewGuid().ToString("N"),
                battleId = battleId,
                randomSeed = randomSeed,
                recordedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            IsRecording = true;
        }

        public void RecordCommand(int turnIndex, string casterId, string skillId, IEnumerable<string> targetIds)
        {
            if (!IsRecording || ActiveReplay == null) return;

            ActiveReplay.commands.Add(new BattleReplayCommand
            {
                turnIndex = turnIndex,
                casterId = casterId,
                skillId = skillId,
                targetIds = new List<string>(targetIds),
                timestampSeconds = Time.realtimeSinceStartup
            });
        }

        public BattleReplayData StopRecording()
        {
            IsRecording = false;
            return ActiveReplay;
        }

        public void LoadReplay(BattleReplayData replay)
        {
            ActiveReplay = replay;
            playbackIndex = 0;
            IsPlaying = false;
            IsPaused = false;
        }

        public void Play()
        {
            if (ActiveReplay == null) return;
            IsPlaying = true;
            IsPaused = false;
        }

        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;

        // Caller-driven playback (same shape as BossPhaseController.EvaluateHpPercent): the UI feeds
        // in elapsed playback time each frame and gets back the next due command, if any, so this
        // class never has to own a MonoBehaviour Update loop itself.
        public bool TryConsumeNextCommand(float elapsedPlaybackSeconds, out BattleReplayCommand command)
        {
            command = null;
            if (!IsPlaying || IsPaused || ActiveReplay == null) return false;

            if (playbackIndex >= ActiveReplay.commands.Count)
            {
                IsPlaying = false;
                OnPlaybackFinished?.Invoke();
                return false;
            }

            var next = ActiveReplay.commands[playbackIndex];
            if (next.timestampSeconds > elapsedPlaybackSeconds) return false;

            playbackIndex++;
            command = next;
            OnCommandPlayed?.Invoke(next);
            return true;
        }
    }
}
