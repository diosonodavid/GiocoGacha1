using System.Collections;
using System.Collections.Generic;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.MiniGames
{
    // Times a fish bite: after a random wait, a short catch window opens where TryCatch must be
    // called; missing the window ends the attempt with no catch.
    public class FishingMiniGame : BaseMiniGame
    {
        [SerializeField] private Vector2 biteWaitRangeSeconds = new(2f, 6f);
        [SerializeField] private float catchWindowSeconds = 1.5f;
        [SerializeField] private RecipeIngredient fishReward;

        private Coroutine activeRoutine;
        private bool biteWindowOpen;

        public override void StartMiniGame()
        {
            base.StartMiniGame();
            biteWindowOpen = false;
            activeRoutine = StartCoroutine(FishingRoutine());
        }

        public void TryCatch()
        {
            if (!IsActive || !biteWindowOpen) return;

            biteWindowOpen = false;
            if (activeRoutine != null) StopCoroutine(activeRoutine);

            var rewards = fishReward != null ? new List<RecipeIngredient> { fishReward } : new List<RecipeIngredient>();
            EndMiniGame(new MiniGameResult { success = true, score = 1, rewards = rewards });
        }

        private IEnumerator FishingRoutine()
        {
            float waitTime = Random.Range(biteWaitRangeSeconds.x, biteWaitRangeSeconds.y);
            yield return new WaitForSeconds(waitTime);

            biteWindowOpen = true;
            yield return new WaitForSeconds(catchWindowSeconds);

            if (!biteWindowOpen) yield break;

            biteWindowOpen = false;
            EndMiniGame(new MiniGameResult { success = false, score = 0 });
        }
    }
}
