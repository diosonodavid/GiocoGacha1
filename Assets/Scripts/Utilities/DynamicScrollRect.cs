using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.Utilities
{
    // Virtualized vertical list for ScrollRect: only ever instantiates enough item views to cover
    // the visible viewport (+ a buffer row each side) and recycles them as the user scrolls,
    // instead of one GameObject per data row - the difference between smooth and unusable once a
    // list reaches hundreds of entries (e.g. a full gear or inventory screen).
    // Assumes content is top-anchored/top-pivoted and grows downward, and that every row is the
    // same height (itemHeight).
    [RequireComponent(typeof(ScrollRect))]
    public class DynamicScrollRect : MonoBehaviour
    {
        [SerializeField] private RectTransform itemPrefab;
        [SerializeField] private float itemHeight = 100f;
        [SerializeField] private int bufferRows = 1;

        private ScrollRect scrollRect;
        private RectTransform content;
        private readonly List<RectTransform> pooledItems = new();
        private readonly Dictionary<int, RectTransform> activeItemsByIndex = new();

        private int itemCount;
        private Action<int, RectTransform> bindItem;
        private int firstVisibleIndex = -1;
        private int lastVisibleIndex = -1;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            content = scrollRect.content;
            scrollRect.onValueChanged.AddListener(_ => RefreshVisibleItems());
        }

        // Call whenever the data source changes; bindItemCallback populates the recycled row at index.
        public void SetItemCount(int count, Action<int, RectTransform> bindItemCallback)
        {
            itemCount = Mathf.Max(0, count);
            bindItem = bindItemCallback;

            content.sizeDelta = new Vector2(content.sizeDelta.x, itemCount * itemHeight);

            ReleaseAllItems();
            firstVisibleIndex = -1;
            lastVisibleIndex = -1;
            RefreshVisibleItems();
        }

        private void RefreshVisibleItems()
        {
            if (itemPrefab == null || itemCount == 0) return;

            float viewportHeight = scrollRect.viewport.rect.height;
            float scrolledY = -content.anchoredPosition.y;

            int firstIndex = Mathf.Max(0, Mathf.FloorToInt(scrolledY / itemHeight) - bufferRows);
            int lastIndex = Mathf.Min(itemCount - 1, Mathf.CeilToInt((scrolledY + viewportHeight) / itemHeight) + bufferRows);

            if (firstIndex == firstVisibleIndex && lastIndex == lastVisibleIndex) return;

            ReleaseItemsOutsideRange(firstIndex, lastIndex);

            for (int i = firstIndex; i <= lastIndex; i++)
            {
                if (activeItemsByIndex.ContainsKey(i)) continue;

                var item = GetPooledItem();
                PositionItem(item, i);
                bindItem?.Invoke(i, item);
                activeItemsByIndex[i] = item;
            }

            firstVisibleIndex = firstIndex;
            lastVisibleIndex = lastIndex;
        }

        private void PositionItem(RectTransform item, int index)
        {
            item.anchoredPosition = new Vector2(item.anchoredPosition.x, -index * itemHeight);
        }

        private void ReleaseItemsOutsideRange(int firstIndex, int lastIndex)
        {
            var toRelease = new List<int>();
            foreach (var kvp in activeItemsByIndex)
            {
                if (kvp.Key < firstIndex || kvp.Key > lastIndex)
                    toRelease.Add(kvp.Key);
            }

            foreach (var index in toRelease)
            {
                ReleaseItem(activeItemsByIndex[index]);
                activeItemsByIndex.Remove(index);
            }
        }

        private RectTransform GetPooledItem()
        {
            if (pooledItems.Count > 0)
            {
                var item = pooledItems[pooledItems.Count - 1];
                pooledItems.RemoveAt(pooledItems.Count - 1);
                item.gameObject.SetActive(true);
                return item;
            }

            return Instantiate(itemPrefab, content);
        }

        private void ReleaseItem(RectTransform item)
        {
            item.gameObject.SetActive(false);
            pooledItems.Add(item);
        }

        private void ReleaseAllItems()
        {
            foreach (var item in activeItemsByIndex.Values)
                ReleaseItem(item);
            activeItemsByIndex.Clear();
        }
    }
}
