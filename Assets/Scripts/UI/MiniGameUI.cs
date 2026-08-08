using GachaGame.MiniGames;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Generic status/score display shared by the mini-game family; bind whichever BaseMiniGame is
    // currently active and this reflects its start/end events without needing a subclass per game.
    public class MiniGameUI : UIController
    {
        [SerializeField] private Text statusText;
        [SerializeField] private Text scoreText;

        private BaseMiniGame boundMiniGame;

        public void Bind(BaseMiniGame miniGame)
        {
            if (boundMiniGame != null)
            {
                boundMiniGame.OnMiniGameStarted -= HandleMiniGameStarted;
                boundMiniGame.OnMiniGameEnded -= HandleMiniGameEnded;
            }

            boundMiniGame = miniGame;

            if (boundMiniGame != null)
            {
                boundMiniGame.OnMiniGameStarted += HandleMiniGameStarted;
                boundMiniGame.OnMiniGameEnded += HandleMiniGameEnded;
            }
        }

        protected override void OnHidden()
        {
            if (boundMiniGame == null) return;
            boundMiniGame.OnMiniGameStarted -= HandleMiniGameStarted;
            boundMiniGame.OnMiniGameEnded -= HandleMiniGameEnded;
        }

        private void HandleMiniGameStarted()
        {
            if (statusText != null) statusText.text = "Go!";
            if (scoreText != null) scoreText.text = "0";
        }

        private void HandleMiniGameEnded(MiniGameResult result)
        {
            if (statusText != null) statusText.text = result.success ? "Success!" : "Missed!";
            if (scoreText != null) scoreText.text = result.score.ToString();
        }
    }
}
