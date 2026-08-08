using System;
using System.Collections.Generic;
using System.Linq;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Social;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Territory map: one node per bound GuildTerritoryData, labeled with its current controller
    // (from GuildWarManager), plus the active phase and its countdown.
    public class GuildWarUI : UIController
    {
        [SerializeField] private List<GuildTerritoryData> territories = new();
        [SerializeField] private Transform territoryNodeContainer;
        [SerializeField] private GameObject territoryNodePrefab;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text countdownText;

        private GuildWarManager guildWarManager;
        private readonly Dictionary<string, Text> nodeLabelByTerritoryId = new();

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out guildWarManager);

            if (guildWarManager != null)
            {
                guildWarManager.OnPhaseChanged += HandlePhaseChanged;
                guildWarManager.OnTerritoryCaptured += HandleTerritoryCaptured;
            }

            BuildTerritoryNodes();
            RefreshPhase();
        }

        protected override void OnHidden()
        {
            if (guildWarManager == null) return;
            guildWarManager.OnPhaseChanged -= HandlePhaseChanged;
            guildWarManager.OnTerritoryCaptured -= HandleTerritoryCaptured;
        }

        private void Update()
        {
            if (!IsShown || guildWarManager == null || countdownText == null) return;

            if (guildWarManager.CurrentPhase is GuildWarPhase.Inactive or GuildWarPhase.Concluded)
            {
                countdownText.text = string.Empty;
                return;
            }

            long remaining = guildWarManager.PhaseEndTimeUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            countdownText.text = TimeSpan.FromSeconds(Mathf.Max(0, remaining)).ToString(@"hh\:mm\:ss");
        }

        private void BuildTerritoryNodes()
        {
            if (territoryNodeContainer == null || territoryNodePrefab == null) return;

            for (int i = territoryNodeContainer.childCount - 1; i >= 0; i--)
                Destroy(territoryNodeContainer.GetChild(i).gameObject);
            nodeLabelByTerritoryId.Clear();

            foreach (var territory in territories)
            {
                if (territory == null) continue;

                var go = Instantiate(territoryNodePrefab, territoryNodeContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) nodeLabelByTerritoryId[territory.territoryId] = label;

                RefreshNodeLabel(territory.territoryId, territory.territoryName);
            }
        }

        private void HandlePhaseChanged(GuildWarPhase phase) => RefreshPhase();

        private void HandleTerritoryCaptured(string territoryId, string capturingGuildId)
        {
            var territory = territories.FirstOrDefault(t => t != null && t.territoryId == territoryId);
            if (territory != null) RefreshNodeLabel(territoryId, territory.territoryName, capturingGuildId);
        }

        private void RefreshNodeLabel(string territoryId, string territoryName, string controllingGuildId = null)
        {
            if (!nodeLabelByTerritoryId.TryGetValue(territoryId, out var label)) return;

            string owner = controllingGuildId ?? guildWarManager?.GetControllingGuild(territoryId);
            label.text = string.IsNullOrEmpty(owner) ? territoryName : $"{territoryName}\n[{owner}]";
        }

        private void RefreshPhase()
        {
            if (phaseText != null) phaseText.text = guildWarManager != null ? guildWarManager.CurrentPhase.ToString() : string.Empty;
        }
    }
}
