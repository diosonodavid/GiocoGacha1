using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    public class TalentTreeUI : UIController
    {
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private GameObject nodeViewPrefab;
        [SerializeField] private GameObject connectionLinePrefab;
        [SerializeField] private List<TalentNodeData> treeNodes = new();
        [SerializeField] private Text talentPointsText;
        [SerializeField] private Button unlockButton;

        private TalentTreeManager talentTreeManager;
        private CharacterInstance targetInstance;
        private TalentNodeData selectedNode;
        private readonly List<TalentNodeView> spawnedViews = new();
        private readonly List<RectTransform> spawnedConnections = new();

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out talentTreeManager);
            if (unlockButton != null) unlockButton.onClick.AddListener(HandleUnlockPressed);
        }

        protected override void OnHidden()
        {
            if (unlockButton != null) unlockButton.onClick.RemoveListener(HandleUnlockPressed);
        }

        public void BindCharacter(CharacterInstance instance)
        {
            targetInstance = instance;
            selectedNode = null;
            RebuildTree();
        }

        private void RebuildTree()
        {
            foreach (var view in spawnedViews)
                if (view != null) Destroy(view.gameObject);
            spawnedViews.Clear();

            foreach (var connection in spawnedConnections)
                if (connection != null) Destroy(connection.gameObject);
            spawnedConnections.Clear();

            if (nodeContainer == null || nodeViewPrefab == null) return;

            var rectByNode = new Dictionary<TalentNodeData, RectTransform>();

            foreach (var node in treeNodes)
            {
                if (node == null) continue;

                var go = Instantiate(nodeViewPrefab, nodeContainer);
                var view = go.GetComponent<TalentNodeView>();
                if (view == null) continue;

                view.Bind(node, GetNodeState(node), HandleNodeSelected);
                spawnedViews.Add(view);
                rectByNode[node] = go.GetComponent<RectTransform>();
            }

            DrawConnections(rectByNode);
            RefreshPointsText();
        }

        private void DrawConnections(Dictionary<TalentNodeData, RectTransform> rectByNode)
        {
            if (connectionLinePrefab == null) return;

            foreach (var node in treeNodes)
            {
                if (node == null || !rectByNode.TryGetValue(node, out var nodeRect)) continue;

                foreach (var prerequisite in node.prerequisites)
                {
                    if (prerequisite == null || !rectByNode.TryGetValue(prerequisite, out var prerequisiteRect)) continue;

                    var connectorGo = Instantiate(connectionLinePrefab, nodeContainer);
                    var connectorRect = connectorGo.GetComponent<RectTransform>();
                    if (connectorRect == null) continue;

                    AlignConnector(connectorRect, prerequisiteRect.anchoredPosition, nodeRect.anchoredPosition);
                    spawnedConnections.Add(connectorRect);
                }
            }
        }

        private static void AlignConnector(RectTransform connector, Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            connector.anchoredPosition = from + direction * 0.5f;
            connector.sizeDelta = new Vector2(direction.magnitude, connector.sizeDelta.y);
            connector.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        }

        private TalentNodeState GetNodeState(TalentNodeData node)
        {
            if (talentTreeManager == null || targetInstance == null) return TalentNodeState.Locked;
            if (talentTreeManager.IsNodeUnlocked(targetInstance, node)) return TalentNodeState.Unlocked;
            return talentTreeManager.CanUnlockNode(targetInstance, node) ? TalentNodeState.Unlockable : TalentNodeState.Locked;
        }

        private void HandleNodeSelected(TalentNodeData node) => selectedNode = node;

        private void HandleUnlockPressed()
        {
            if (talentTreeManager == null || targetInstance == null || selectedNode == null) return;
            if (talentTreeManager.TryUnlockNode(targetInstance, selectedNode))
                RebuildTree();
        }

        private void RefreshPointsText()
        {
            if (talentPointsText != null && targetInstance != null)
                talentPointsText.text = targetInstance.talentPoints.ToString();
        }
    }
}
