using System;
using System.Collections.Generic;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.MiniGames
{
    [Serializable]
    public class MiniGameResult
    {
        public bool success;
        public int score;
        public List<RecipeIngredient> rewards = new();
    }

    // Common lifecycle for secondary activities (fishing, dice, slots, ...): StartMiniGame begins an
    // attempt, subclasses drive their own timing/input, and EndMiniGame reports the outcome via
    // OnMiniGameEnded for a UI and MiniGameRewardHandler to react to.
    public abstract class BaseMiniGame : MonoBehaviour
    {
        public event Action OnMiniGameStarted;
        public event Action<MiniGameResult> OnMiniGameEnded;

        public bool IsActive { get; protected set; }

        public virtual void StartMiniGame()
        {
            IsActive = true;
            OnMiniGameStarted?.Invoke();
        }

        protected void EndMiniGame(MiniGameResult result)
        {
            IsActive = false;
            OnMiniGameEnded?.Invoke(result);
        }
    }
}
