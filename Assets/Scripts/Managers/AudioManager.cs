using System.Threading.Tasks;
using GachaGame.Core;
using UnityEngine;

namespace GachaGame.Managers
{
    // Three independent channels (BGM/SFX/Voice) driven purely through AudioSource.volume, so it
    // works with or without an AudioMixer asset assigned in the scene.
    public class AudioManager : MonoBehaviour, IService
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voiceSource;

        [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float voiceVolume = 1f;

        public float BgmVolume => bgmVolume;
        public float SfxVolume => sfxVolume;
        public float VoiceVolume => voiceVolume;

        public Task InitializeAsync()
        {
            ApplyVolumes();
            Debug.Log($"{nameof(AudioManager)} initialized.");
            return Task.CompletedTask;
        }

        public Task ShutdownAsync() => Task.CompletedTask;

        public void SetBgmVolume(float value)
        {
            bgmVolume = Mathf.Clamp01(value);
            if (bgmSource != null) bgmSource.volume = bgmVolume;
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }

        public void SetVoiceVolume(float value)
        {
            voiceVolume = Mathf.Clamp01(value);
            if (voiceSource != null) voiceSource.volume = voiceVolume;
        }

        public void PlayBgm(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null || clip == null) return;
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlayVoice(AudioClip clip)
        {
            if (voiceSource == null || clip == null) return;
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = voiceVolume;
            voiceSource.Play();
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null) bgmSource.volume = bgmVolume;
            if (sfxSource != null) sfxSource.volume = sfxVolume;
            if (voiceSource != null) voiceSource.volume = voiceVolume;
        }
    }
}
