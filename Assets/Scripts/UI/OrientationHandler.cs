using UnityEngine;

namespace GachaGame.UI
{
    // Thin wrapper around Screen.orientation/autorotation so a single component can lock the game
    // to Portrait or Landscape from a settings toggle or a per-scene default, instead of scattering
    // Screen.orientation assignments across scene-specific scripts.
    public class OrientationHandler : MonoBehaviour
    {
        [SerializeField] private ScreenOrientation defaultOrientation = ScreenOrientation.Portrait;
        [SerializeField] private bool applyOnAwake = true;

        private void Awake()
        {
            if (applyOnAwake) Lock(defaultOrientation);
        }

        public void Lock(ScreenOrientation orientation)
        {
            Screen.orientation = orientation;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }

        public void Unlock()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
    }
}
