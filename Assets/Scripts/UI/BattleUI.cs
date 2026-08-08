using System;
using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Top-level battle HUD: renders the Action Gauge turn order, binds a BattlePortraitWidget per
    // combatant, and exposes the active character's skill buttons. Hooks into
    // BattleTurnController's events rather than polling battle state every frame.
    public class BattleUI : MonoBehaviour
    {
        [SerializeField] private Transform turnOrderContainer;
        [SerializeField] private GameObject turnOrderIconPrefab;
        [SerializeField] private Transform skillButtonContainer;
        [SerializeField] private Button skillButtonPrefab;
        [SerializeField] private DamageTextSpawner damageTextSpawner;

        private BattleTurnController battleTurnController;
        private readonly Dictionary<ICombatant, BattlePortraitWidget> widgetsByCombatant = new();

        public void Bind(BattleTurnController controller)
        {
            Unbind();
            battleTurnController = controller;
            if (battleTurnController == null) return;

            battleTurnController.OnTurnChanged += HandleTurnChanged;
            battleTurnController.OnCharacterDied += HandleCharacterDied;
        }

        public void Unbind()
        {
            if (battleTurnController == null) return;
            battleTurnController.OnTurnChanged -= HandleTurnChanged;
            battleTurnController.OnCharacterDied -= HandleCharacterDied;
            battleTurnController = null;
        }

        private void OnDisable() => Unbind();

        public void RegisterPortrait(ICombatant combatant, BattlePortraitWidget widget)
        {
            if (combatant == null || widget == null) return;
            widgetsByCombatant[combatant] = widget;
        }

        public void RefreshAllPortraits()
        {
            foreach (var widget in widgetsByCombatant.Values)
                widget.Refresh();
        }

        public void ShowFloatingDamage(ICombatant target, Vector3 worldPosition, DamageResult result)
        {
            damageTextSpawner?.Spawn(worldPosition, result);
            if (widgetsByCombatant.TryGetValue(target, out var widget))
                widget.Refresh();
        }

        public void SetSkillButtons(IReadOnlyList<SkillData> skills, Action<SkillData> onSkillSelected)
        {
            if (skillButtonContainer == null || skillButtonPrefab == null) return;

            for (int i = skillButtonContainer.childCount - 1; i >= 0; i--)
                Destroy(skillButtonContainer.GetChild(i).gameObject);

            foreach (var skill in skills)
            {
                var button = Instantiate(skillButtonPrefab, skillButtonContainer);
                var label = button.GetComponentInChildren<Text>();
                if (label != null) label.text = skill.skillName;
                button.onClick.AddListener(() => onSkillSelected?.Invoke(skill));
            }
        }

        private void HandleTurnChanged(ICombatant activeCombatant)
        {
            RefreshAllPortraits();
            RefreshTurnOrderDisplay();
        }

        private void HandleCharacterDied(ICombatant combatant)
        {
            if (widgetsByCombatant.TryGetValue(combatant, out var widget))
                widget.Refresh();
        }

        private void RefreshTurnOrderDisplay()
        {
            // The predicted queue order requires re-simulating TurnManager's internal state, which
            // is intentionally not exposed publicly; this redraws a simple "who is alive" strip
            // until a read-only queue preview is added to TurnManager.
            if (turnOrderContainer == null || turnOrderIconPrefab == null) return;

            for (int i = turnOrderContainer.childCount - 1; i >= 0; i--)
                Destroy(turnOrderContainer.GetChild(i).gameObject);

            foreach (var combatant in widgetsByCombatant.Keys)
            {
                if (!combatant.IsAlive) continue;
                Instantiate(turnOrderIconPrefab, turnOrderContainer);
            }
        }
    }
}
