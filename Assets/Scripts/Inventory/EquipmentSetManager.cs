using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Groups a character's equipped runes by EquipmentSetData.setId and returns the combined stat
    // bonuses for every set threshold reached (2pc, and 4pc as well once 4+ are worn - both apply
    // together, matching common gacha set-bonus conventions). This is a computation-only service:
    // applying the result into combat stats is a future StatCalculator integration point, mirroring
    // how CharacterAscensionSystem's statPercentBonus is stored without being auto-applied.
    public class EquipmentSetManager : MonoBehaviour, IService
    {
        [SerializeField] private List<EquipmentSetData> setCatalog = new();

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(EquipmentSetManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public Dictionary<StatType, float> GetActiveSetBonuses(CharacterInstance instance)
        {
            var result = new Dictionary<StatType, float>();
            if (instance == null) return result;

            var pieceCountBySetId = instance.equippedRunes.Dictionary.Values
                .Where(rune => rune != null && !string.IsNullOrEmpty(rune.setId))
                .GroupBy(rune => rune.setId)
                .ToDictionary(group => group.Key, group => group.Count());

            foreach (var kvp in pieceCountBySetId)
            {
                var setData = setCatalog.FirstOrDefault(s => s != null && s.setId == kvp.Key);
                if (setData == null) continue;

                if (kvp.Value >= 2) ApplyBonus(result, setData.twoPieceBonus);
                if (kvp.Value >= 4) ApplyBonus(result, setData.fourPieceBonus);
            }

            return result;
        }

        private static void ApplyBonus(Dictionary<StatType, float> result, List<GearStat> bonus)
        {
            if (bonus == null) return;
            foreach (var stat in bonus)
            {
                if (stat == null) continue;
                result[stat.statType] = (result.TryGetValue(stat.statType, out var existing) ? existing : 0f) + stat.value;
            }
        }
    }
}
