using System;
using System.Collections.Generic;
using UnityEngine;

namespace GachaGame.Data
{
    [Serializable]
    public class CharacterGiftBonus
    {
        public string characterId;
        public int bonusAffinityPoints;
    }

    // Extends the generic item catalog with per-character affinity payouts; GiveGift looks up
    // characterBonuses for the recipient before falling back to baseAffinityPoints.
    [CreateAssetMenu(fileName = "GiftItemData", menuName = "GachaGame/Data/Gift Item Data")]
    public class GiftItemData : ItemData
    {
        public int baseAffinityPoints = 10;
        public List<CharacterGiftBonus> characterBonuses = new();

        public int GetAffinityPoints(string characterId)
        {
            var bonus = characterBonuses.Find(b => b.characterId == characterId);
            return bonus != null ? baseAffinityPoints + bonus.bonusAffinityPoints : baseAffinityPoints;
        }
    }
}
