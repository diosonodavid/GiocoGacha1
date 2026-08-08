using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    [Serializable]
    public class HardwareInfoSnapshot
    {
        public string deviceModel;
        public string operatingSystem;
        public string graphicsDeviceName;
        public int systemMemoryMb;
        public int graphicsMemoryMb;
        public int processorCount;
    }

    // Collects a snapshot of the device's hardware once at startup, for logging/diagnostics and as
    // data other systems (DevicePerformanceScaler, PostProcessingController) can read to make
    // rendering decisions - this class only gathers and exposes the snapshot, it doesn't apply any
    // quality changes itself.
    public class HardwareInfoLogger : MonoBehaviour, IService
    {
        public HardwareInfoSnapshot Snapshot { get; private set; }

        public Task InitializeAsync()
        {
            Snapshot = new HardwareInfoSnapshot
            {
                deviceModel = SystemInfo.deviceModel,
                operatingSystem = SystemInfo.operatingSystem,
                graphicsDeviceName = SystemInfo.graphicsDeviceName,
                systemMemoryMb = SystemInfo.systemMemorySize,
                graphicsMemoryMb = SystemInfo.graphicsMemorySize,
                processorCount = SystemInfo.processorCount
            };

            Debug.Log($"{nameof(HardwareInfoLogger)}: {Snapshot.deviceModel} | {Snapshot.graphicsDeviceName} | RAM {Snapshot.systemMemoryMb}MB | CPUs {Snapshot.processorCount}");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;
    }
}
