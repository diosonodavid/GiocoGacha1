using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // Debug-only lightweight profiler: rolling FPS average, GC-tracked managed memory, and a
    // periodic ICMP ping to gauge network latency - polled by DebugMenuUI rather than logged
    // continuously, to avoid spamming the console in normal play.
    public class PerformanceMonitor : MonoBehaviour, IService
    {
        [SerializeField] private string pingHost = "8.8.8.8";
        [SerializeField] private float pingIntervalSeconds = 5f;

        private float frameTimeAccumulator;
        private int frameCount;
        private float pingTimer;
        private Ping activePing;

        public float CurrentFps { get; private set; }
        public long UsedMemoryBytes => System.GC.GetTotalMemory(false);
        public int LastPingMilliseconds { get; private set; } = -1;

        public Task InitializeAsync()
        {
            Debug.Log($"{nameof(PerformanceMonitor)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        private void Update()
        {
            frameTimeAccumulator += Time.unscaledDeltaTime;
            frameCount++;

            if (frameTimeAccumulator >= 0.5f)
            {
                CurrentFps = frameCount / frameTimeAccumulator;
                frameTimeAccumulator = 0f;
                frameCount = 0;
            }

            UpdatePing();
        }

        private void UpdatePing()
        {
            if (activePing != null)
            {
                if (!activePing.isDone) return;
                LastPingMilliseconds = activePing.time;
                activePing = null;
            }

            pingTimer += Time.unscaledDeltaTime;
            if (pingTimer < pingIntervalSeconds) return;

            pingTimer = 0f;
            activePing = new Ping(pingHost);
        }
    }
}
