using System;
using System.Threading.Tasks;
using GachaGame.Networking;
using UnityEngine;

namespace GachaGame.Core
{
    [Serializable]
    public class CrashReportPayload
    {
        public string exceptionMessage;
        public string stackTrace;
        public string deviceModel;
        public string appVersion;
        public long timestampUnix;
    }

    // Unity surfaces uncaught exceptions (native crashes aside) through
    // Application.logMessageReceived as LogType.Exception, so that's the hook used here rather
    // than a native crash handler; each report is sent as soon as it's captured, since a crash is
    // exactly the moment a later "flush on shutdown" step might not run.
    public class CrashReportService : MonoBehaviour, IService
    {
        private NetworkManager networkManager;

        public Task InitializeAsync()
        {
            ServiceLocator.Instance.TryGet(out networkManager);
            Application.logMessageReceived += HandleLog;
            Debug.Log($"{nameof(CrashReportService)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            Application.logMessageReceived -= HandleLog;
            return Task.CompletedTask;
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception) return;

            var payload = new CrashReportPayload
            {
                exceptionMessage = condition,
                stackTrace = stackTrace,
                deviceModel = SystemInfo.deviceModel,
                appVersion = Application.version,
                timestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _ = SendReportAsync(payload);
        }

        public async Task<bool> SendReportAsync(CrashReportPayload payload)
        {
            if (networkManager == null || payload == null) return false;

            var response = await networkManager.PostAsync<object>("/diagnostics/crash-report", payload);
            return response.success;
        }
    }
}
