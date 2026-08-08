using System;
using System.Threading.Tasks;
using GachaGame.Combat;
using GachaGame.Core;
using GachaGame.Network;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Browse-and-watch screen for the leaderboard's top replays: lists ReplaySyncService's
    // summaries, downloads the selected one, and loads it into a bound BattleReplayManager for
    // BattleReplayUI to play back.
    public class TopPlayersReplayUI : UIController
    {
        [SerializeField] private Transform replayListContainer;
        [SerializeField] private GameObject replayEntryPrefab;

        private ReplaySyncService replaySyncService;
        private BattleReplayManager replayManager;

        public event Action<BattleReplayData> OnReplaySelected;

        public void BindPlaybackManager(BattleReplayManager manager) => replayManager = manager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out replaySyncService);
            _ = RefreshListAsync();
        }

        private async Task RefreshListAsync()
        {
            if (replaySyncService == null || replayListContainer == null || replayEntryPrefab == null) return;

            var summaries = await replaySyncService.FetchTopReplaysAsync();

            ClearContainer(replayListContainer);
            foreach (var summary in summaries)
            {
                var go = Instantiate(replayEntryPrefab, replayListContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = $"#{summary.rank}  {summary.ownerName}";

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => _ = HandleReplaySelectedAsync(summary.replayId));
            }
        }

        private async Task HandleReplaySelectedAsync(string replayId)
        {
            if (replaySyncService == null) return;

            var data = await replaySyncService.DownloadReplayAsync(replayId);
            if (data == null) return;

            replayManager?.LoadReplay(data);
            OnReplaySelected?.Invoke(data);
        }

        private static void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
