using System;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public enum TalentNodeState
    {
        Locked,
        Unlockable,
        Unlocked
    }

    // Single node icon within TalentTreeUI's grid; visual state is a color swap on an overlay
    // image rather than separate prefabs per state, so art can be tuned without touching layout.
    public class TalentNodeView : MonoBehaviour
    {
        [SerializeField] private Image stateOverlay;
        [SerializeField] private Button selectButton;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color unlockableColor = Color.yellow;
        [SerializeField] private Color unlockedColor = Color.white;

        public TalentNodeData Node { get; private set; }

        public void Bind(TalentNodeData node, TalentNodeState state, Action<TalentNodeData> onSelected)
        {
            Node = node;
            ApplyState(state);

            if (selectButton == null) return;
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelected?.Invoke(node));
        }

        public void ApplyState(TalentNodeState state)
        {
            if (stateOverlay == null) return;
            stateOverlay.color = state switch
            {
                TalentNodeState.Locked => lockedColor,
                TalentNodeState.Unlockable => unlockableColor,
                _ => unlockedColor
            };
        }
    }
}
