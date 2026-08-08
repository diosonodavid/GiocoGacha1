using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GachaGame.Core;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Inventory
{
    // Owns every saved team loadout plus which one is currently active. TeamUI edits a loadout's
    // slots through this rather than mutating TeamData directly, so ownership checks (does the
    // player actually have this character?) happen in one place.
    public class TeamManager : MonoBehaviour, IService
    {
        public event Action<int> OnActiveTeamChanged;
        public event Action<int, int> OnTeamSlotChanged; // (teamIndex, slotIndex)

        [SerializeField] private List<TeamData> savedTeams = new();
        [SerializeField] private int activeTeamIndex;

        private InventoryManager inventoryManager;

        public IReadOnlyList<TeamData> SavedTeams => savedTeams;
        public int ActiveTeamIndex => activeTeamIndex;
        public TeamData ActiveTeam => activeTeamIndex >= 0 && activeTeamIndex < savedTeams.Count ? savedTeams[activeTeamIndex] : null;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out inventoryManager);
            if (savedTeams.Count == 0)
                savedTeams.Add(new TeamData { teamId = Guid.NewGuid().ToString(), teamName = "Team 1" });

            Debug.Log($"{nameof(TeamManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public TeamData AddTeam(string teamName)
        {
            var team = new TeamData { teamId = Guid.NewGuid().ToString(), teamName = teamName };
            savedTeams.Add(team);
            return team;
        }

        public void RemoveTeam(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= savedTeams.Count || savedTeams.Count <= 1) return;

            savedTeams.RemoveAt(teamIndex);
            if (activeTeamIndex >= savedTeams.Count) SetActiveTeam(savedTeams.Count - 1);
        }

        public void SetActiveTeam(int teamIndex)
        {
            if (teamIndex < 0 || teamIndex >= savedTeams.Count || teamIndex == activeTeamIndex) return;

            activeTeamIndex = teamIndex;
            OnActiveTeamChanged?.Invoke(activeTeamIndex);
        }

        // Places baseDataId in the slot, only if the player actually owns that character and it
        // isn't already assigned to a different slot on the same team.
        public bool TrySetSlot(int teamIndex, int slotIndex, string baseDataId)
        {
            if (!TryGetTeam(teamIndex, out var team)) return false;
            if (slotIndex < 0 || slotIndex >= TeamData.MaxSlots) return false;

            if (!string.IsNullOrEmpty(baseDataId))
            {
                if (inventoryManager != null && !inventoryManager.HasCharacter(baseDataId)) return false;
                if (Array.IndexOf(team.memberBaseDataIds, baseDataId) >= 0) return false;
            }

            team.memberBaseDataIds[slotIndex] = baseDataId;
            OnTeamSlotChanged?.Invoke(teamIndex, slotIndex);
            return true;
        }

        public void ClearSlot(int teamIndex, int slotIndex) => TrySetSlot(teamIndex, slotIndex, null);

        public bool SwapSlots(int teamIndex, int slotIndexA, int slotIndexB)
        {
            if (!TryGetTeam(teamIndex, out var team)) return false;
            if (slotIndexA < 0 || slotIndexA >= TeamData.MaxSlots || slotIndexB < 0 || slotIndexB >= TeamData.MaxSlots) return false;

            (team.memberBaseDataIds[slotIndexA], team.memberBaseDataIds[slotIndexB]) =
                (team.memberBaseDataIds[slotIndexB], team.memberBaseDataIds[slotIndexA]);

            OnTeamSlotChanged?.Invoke(teamIndex, slotIndexA);
            OnTeamSlotChanged?.Invoke(teamIndex, slotIndexB);
            return true;
        }

        private bool TryGetTeam(int teamIndex, out TeamData team)
        {
            team = teamIndex >= 0 && teamIndex < savedTeams.Count ? savedTeams[teamIndex] : null;
            return team != null;
        }
    }
}
