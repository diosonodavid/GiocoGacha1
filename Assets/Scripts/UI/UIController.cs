using UnityEngine;

namespace GachaGame.UI
{
    // Common show/hide contract for full-screen or panel UI screens, so navigation code can
    // transition between them uniformly regardless of what each screen actually displays.
    public abstract class UIController : MonoBehaviour
    {
        public bool IsShown { get; private set; }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            IsShown = true;
            OnShown();
        }

        public virtual void Hide()
        {
            OnHidden();
            IsShown = false;
            gameObject.SetActive(false);
        }

        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
}
