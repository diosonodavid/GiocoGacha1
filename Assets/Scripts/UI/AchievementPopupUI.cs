using System.Collections;
using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.UI
{
    // Listens for AchievementManager.OnAchievementUnlocked and queues a popup per unlock, so
    // several trophies completing in the same frame (e.g. after a big battle reward) still show
    // one at a time instead of overlapping.
    public class AchievementPopupUI : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private float displaySeconds = 3f;

        private static readonly int ShowTrigger = Animator.StringToHash("Show");

        private readonly Queue<AchievementData> pendingPopups = new();
        private bool isShowingPopup;

        private void OnEnable()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementUnlocked += Enqueue;

            if (popupRoot != null) popupRoot.SetActive(false);
        }

        private void OnDisable()
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.OnAchievementUnlocked -= Enqueue;
        }

        private void Enqueue(AchievementData achievement)
        {
            if (achievement == null) return;
            pendingPopups.Enqueue(achievement);
            if (!isShowingPopup) StartCoroutine(ShowNextPopup());
        }

        private IEnumerator ShowNextPopup()
        {
            isShowingPopup = true;

            while (pendingPopups.Count > 0)
            {
                var achievement = pendingPopups.Dequeue();

                if (titleText != null) titleText.text = achievement.title;
                if (descriptionText != null) descriptionText.text = achievement.description;

                if (popupRoot != null) popupRoot.SetActive(true);
                popupAnimator?.SetTrigger(ShowTrigger);

                yield return new WaitForSeconds(displaySeconds);

                if (popupRoot != null) popupRoot.SetActive(false);
            }

            isShowingPopup = false;
        }
    }
}
