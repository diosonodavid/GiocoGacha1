using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GachaGame.Core
{
    // Lowers target frame rate and screen brightness when the player has gone idle (or has left
    // auto-battle running unattended) past a timeout, and restores them once activity resumes.
    // Callers (input handlers, AutoBattleController's driver) are responsible for calling
    // NotifyPlayerActivity - this class doesn't know what "activity" means, same restraint as
    // BossPhaseController/GemCombineService's caller-driven designs.
    public class BatterySaverMode : MonoBehaviour, IService
    {
        [SerializeField] private int reducedFrameRate = 30;
        [SerializeField] private float reducedBrightness = 0.5f;
        [SerializeField] private float idleTimeoutSeconds = 60f;

        public event Action<bool> OnBatterySaverToggled;

        private int normalFrameRate;
        private float normalBrightness;
        private float idleTimer;

        public bool IsActive { get; private set; }

        public Task InitializeAsync()
        {
            normalFrameRate = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60;
            normalBrightness = Screen.brightness;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void NotifyPlayerActivity() => idleTimer = 0f;

        private void Update()
        {
            idleTimer += Time.deltaTime;
            bool shouldBeActive = idleTimer >= idleTimeoutSeconds;
            if (shouldBeActive != IsActive) SetActive(shouldBeActive);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            Application.targetFrameRate = active ? reducedFrameRate : normalFrameRate;

            // Screen.brightness is only settable on platforms that support it (mainly iOS); a
            // negative value back from the getter means the platform doesn't support it at all.
            if (normalBrightness >= 0f) Screen.brightness = active ? reducedBrightness : normalBrightness;

            OnBatterySaverToggled?.Invoke(active);
        }
    }
}
