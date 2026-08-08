using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [CreateAssetMenu(fileName = "ExpeditionData", menuName = "GachaGame/Data/Expedition Data")]
    public class ExpeditionData : ScriptableObject
    {
        public string expeditionId;
        public string expeditionName;
        public int durationMinutes;
        public ElementType recommendedElement;
        public int teamCapacity = 3;
        public List<RecipeIngredient> rewardItems = new();
    }
}
