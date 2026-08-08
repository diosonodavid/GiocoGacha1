using System.Collections;
using GachaGame.Core;
using GachaGame.Managers;
using UnityEngine;

namespace GachaGame.Audio
{
    // Crossfades between BGM tracks based on combat intensity (calm/battle/boss) using its own pair
    // of AudioSources rather than AudioManager's single bgmSource, since a one-slot source can't
    // hold two overlapping clips mid-fade; still reads AudioManager.BgmVolume so the player's slider
    // keeps applying to whichever track is currently playing.
    public class DynamicBGMController : MonoBehaviour
    {
        [SerializeField] private AudioSource sourceA;
        [SerializeField] private AudioSource sourceB;
        [SerializeField] private float crossfadeDurationSeconds = 2f;
        [SerializeField] private AudioClip calmTrack;
        [SerializeField] private AudioClip battleTrack;
        [SerializeField] private AudioClip bossTrack;

        private AudioManager audioManager;
        private AudioSource activeSource;
        private AudioSource inactiveSource;
        private Coroutine crossfadeRoutine;

        private void Awake()
        {
            activeSource = sourceA;
            inactiveSource = sourceB;
        }

        private void Start() => ServiceLocator.Instance.TryGet(out audioManager);

        public void SetCombatState(bool inBattle, bool isBossFight = false)
        {
            AudioClip target = isBossFight ? bossTrack : inBattle ? battleTrack : calmTrack;
            CrossfadeTo(target);
        }

        public void CrossfadeTo(AudioClip clip)
        {
            if (clip == null || activeSource == null || inactiveSource == null || clip == activeSource.clip) return;

            if (crossfadeRoutine != null) StopCoroutine(crossfadeRoutine);
            crossfadeRoutine = StartCoroutine(CrossfadeRoutine(clip));
        }

        private IEnumerator CrossfadeRoutine(AudioClip clip)
        {
            float targetVolume = audioManager != null ? audioManager.BgmVolume : 1f;

            inactiveSource.clip = clip;
            inactiveSource.volume = 0f;
            inactiveSource.loop = true;
            inactiveSource.Play();

            float elapsed = 0f;
            while (elapsed < crossfadeDurationSeconds)
            {
                elapsed += Time.deltaTime;
                float t = crossfadeDurationSeconds > 0f ? elapsed / crossfadeDurationSeconds : 1f;
                inactiveSource.volume = Mathf.Lerp(0f, targetVolume, t);
                activeSource.volume = Mathf.Lerp(targetVolume, 0f, t);
                yield return null;
            }

            activeSource.Stop();
            (activeSource, inactiveSource) = (inactiveSource, activeSource);
            crossfadeRoutine = null;
        }
    }
}
