using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Global toast queue (ToastNotificationUI.Show("Stamina insufficiente")) for short transient
    // messages any system can trigger without going through ServiceLocator - a classic singleton
    // like AchievementManager, for the same reason: callers can be anywhere, not just other UI.
    public class ToastNotificationUI : MonoBehaviour
    {
        public static ToastNotificationUI Instance { get; private set; }

        [SerializeField] private GameObject toastRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private float displaySeconds = 2f;

        private readonly Queue<string> pendingMessages = new();
        private bool isShowingToast;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (toastRoot != null) toastRoot.SetActive(false);
        }

        public static void Show(string message)
        {
            if (Instance != null) Instance.Enqueue(message);
        }

        private void Enqueue(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            pendingMessages.Enqueue(message);
            if (!isShowingToast) StartCoroutine(ShowNextToast());
        }

        private IEnumerator ShowNextToast()
        {
            isShowingToast = true;

            while (pendingMessages.Count > 0)
            {
                string message = pendingMessages.Dequeue();
                if (messageText != null) messageText.text = message;
                if (toastRoot != null) toastRoot.SetActive(true);

                yield return new WaitForSeconds(displaySeconds);

                if (toastRoot != null) toastRoot.SetActive(false);
            }

            isShowingToast = false;
        }
    }
}
