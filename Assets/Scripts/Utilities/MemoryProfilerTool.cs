using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace GachaGame.Utilities
{
    // Logs total allocated memory (and its delta since the last sample) on every scene load, so a
    // steadily growing delta across transitions is an easy signal to spot a leak during testing.
    public class MemoryProfilerTool : MonoBehaviour
    {
        [SerializeField] private bool logOnSceneTransition = true;

        private long lastRecordedMemoryBytes;

        private void OnEnable()
        {
            if (logOnSceneTransition) SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => LogMemoryDelta(scene.name);

        public void LogMemoryDelta(string context)
        {
            long current = Profiler.GetTotalAllocatedMemoryLong();
            long delta = current - lastRecordedMemoryBytes;
            lastRecordedMemoryBytes = current;

            Debug.Log($"[MemoryProfilerTool] {context}: {current / 1024f / 1024f:0.0} MB allocated ({(delta >= 0 ? "+" : "")}{delta / 1024f / 1024f:0.0} MB since last sample).");
        }
    }
}
