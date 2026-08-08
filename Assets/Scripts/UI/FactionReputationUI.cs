using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class FactionReputationUI : UIController
    {
        [SerializeField] private List<FactionData> factions = new();
        [SerializeField] private Transform factionListContainer;
        [SerializeField] private GameObject factionEntryPrefab;

        private FactionReputationManager reputationManager;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out reputationManager);
            if (reputationManager != null) reputationManager.OnReputationRankChanged += HandleRankChanged;
            RebuildList();
        }

        protected override void OnHidden()
        {
            if (reputationManager != null) reputationManager.OnReputationRankChanged -= HandleRankChanged;
        }

        private void HandleRankChanged(string factionId, int newRankIndex) => RebuildList();

        private void RebuildList()
        {
            ClearContainer();
            if (factionListContainer == null || factionEntryPrefab == null) return;

            foreach (var faction in factions)
            {
                if (faction == null) continue;

                int rankIndex = reputationManager != null ? reputationManager.GetRankIndex(faction.factionId) : -1;
                int points = reputationManager != null ? reputationManager.GetPoints(faction.factionId) : 0;
                string rankName = rankIndex >= 0 && rankIndex < faction.ranks.Count ? faction.ranks[rankIndex].rankName : "Unranked";

                var entry = Instantiate(factionEntryPrefab, factionListContainer);
                var label = entry.GetComponentInChildren<Text>();
                if (label != null) label.text = $"{faction.factionName}: {rankName} ({points} rep)";
            }
        }

        private void ClearContainer()
        {
            if (factionListContainer == null) return;
            for (int i = factionListContainer.childCount - 1; i >= 0; i--)
                Destroy(factionListContainer.GetChild(i).gameObject);
        }
    }
}
