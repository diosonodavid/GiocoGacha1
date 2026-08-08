using System.Collections.Generic;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.MiniGames
{
    // A fixed number of two-die rolls accumulate a score; once rolls run out, every full
    // pointsPerRewardTier of score earns one copy of rewardPerTier.
    public class DiceRollMiniGame : BaseMiniGame
    {
        [SerializeField] private int maxRolls = 3;
        [SerializeField] private int pointsPerRewardTier = 10;
        [SerializeField] private RecipeIngredient rewardPerTier;

        private readonly System.Random random = new();
        private int rollsUsed;
        private int totalScore;

        public override void StartMiniGame()
        {
            base.StartMiniGame();
            rollsUsed = 0;
            totalScore = 0;
        }

        public int RollDice()
        {
            if (!IsActive || rollsUsed >= maxRolls) return 0;

            int roll = random.Next(1, 7) + random.Next(1, 7);
            totalScore += roll;
            rollsUsed++;

            if (rollsUsed >= maxRolls) FinishGame();
            return roll;
        }

        private void FinishGame()
        {
            int tiersEarned = pointsPerRewardTier > 0 ? totalScore / pointsPerRewardTier : 0;
            var rewards = new List<RecipeIngredient>();
            if (rewardPerTier != null && rewardPerTier.item != null && tiersEarned > 0)
                rewards.Add(new RecipeIngredient { item = rewardPerTier.item, amount = rewardPerTier.amount * tiersEarned });

            EndMiniGame(new MiniGameResult { success = true, score = totalScore, rewards = rewards });
        }
    }
}
