using System.Collections.Generic;
using GachaGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Dev-build shortcut panel: jumps straight to any registered screen (bypassing normal
    // navigation flow) and surfaces PerformanceMonitor's live stats without needing InGameConsole.
    public class DebugMenuUI : UIController
    {
        [SerializeField] private List<UIController> testScreens = new();
        [SerializeField] private Transform screenButtonContainer;
        [SerializeField] private GameObject screenButtonPrefab;
        [SerializeField] private Text statsText;
        [SerializeField] private GameObject spawnObjectPrefab;
        [SerializeField] private Transform spawnPoint;

        private PerformanceMonitor performanceMonitor;

        protected override void OnShown()
        {
            ServiceLocator.Instance.TryGet(out performanceMonitor);
            BuildScreenShortcuts();
        }

        private void Update()
        {
            if (!IsShown || statsText == null || performanceMonitor == null) return;
            statsText.text = $"FPS {performanceMonitor.CurrentFps:F0}  |  Mem {performanceMonitor.UsedMemoryBytes / 1_048_576f:F1} MB  |  Ping {performanceMonitor.LastPingMilliseconds} ms";
        }

        private void BuildScreenShortcuts()
        {
            if (screenButtonContainer == null || screenButtonPrefab == null) return;

            for (int i = screenButtonContainer.childCount - 1; i >= 0; i--)
                Destroy(screenButtonContainer.GetChild(i).gameObject);

            foreach (var screen in testScreens)
            {
                if (screen == null) continue;

                var go = Instantiate(screenButtonPrefab, screenButtonContainer);
                var label = go.GetComponentInChildren<Text>();
                if (label != null) label.text = screen.GetType().Name;

                var button = go.GetComponentInChildren<Button>();
                if (button != null) button.onClick.AddListener(() => screen.Show());
            }
        }

        public void SpawnTestObject()
        {
            if (spawnObjectPrefab == null) return;

            Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Instantiate(spawnObjectPrefab, position, Quaternion.identity);
        }
    }
}
