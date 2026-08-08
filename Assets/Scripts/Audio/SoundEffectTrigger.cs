using GachaGame.Core;
using GachaGame.Managers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GachaGame.Audio
{
    // Generic click/hover SFX hook for UI elements - implements the pointer interfaces directly
    // instead of requiring a Button component, so it works on any UI graphic used as a
    // custom button elsewhere in the UI, not just UnityEngine.UI.Button.
    public class SoundEffectTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip hoverClip;

        private AudioManager audioManager;

        private void OnEnable() => ServiceLocator.Instance.TryGet(out audioManager);

        public void OnPointerClick(PointerEventData eventData) => Play(clickClip);

        public void OnPointerEnter(PointerEventData eventData) => Play(hoverClip);

        private void Play(AudioClip clip)
        {
            if (clip == null || audioManager == null) return;
            audioManager.PlaySfx(clip);
        }
    }
}
