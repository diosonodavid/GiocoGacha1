using System.Collections;
using System.Collections.Generic;
using GachaGame.Core;
using GachaGame.Data;
using GachaGame.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GachaGame.Gacha
{
    // Consolidated summon UI: PullOnce/PullTen are meant to be wired directly to the
    // "Pull 1" / "Pull 10" buttons' OnClick() in the Inspector, so every UI-facing method here is
    // public. The actual pull transaction (currency spend, rarity roll, pity, inventory) is owned
    // entirely by GachaManager; this class only plays the reveal animation and renders the result.
    public class GachaUI : MonoBehaviour
    {
        [SerializeField] private Text resultText;
        [SerializeField] private Text pityText;
        [SerializeField] private float revealDelaySeconds = 0.4f;
        [SerializeField] private Animator bannerAnimator;
        [SerializeField] private GameObject resultCardPrefab;
        [SerializeField] private Transform resultCardContainer;
        [SerializeField] private GameObject summaryPanel;

        private static readonly int OpenTrigger = Animator.StringToHash("Open");

        private GachaManager gachaManager;
        private bool isPulling;

        private void OnEnable()
        {
            if (!ServiceLocator.Instance.TryGet(out gachaManager))
            {
                Debug.LogWarning($"{nameof(GachaUI)} could not resolve {nameof(GachaManager)}.", this);
                return;
            }

            RefreshPityDisplay();
        }

        public void PullOnce()
        {
            Debug.Log($"{nameof(GachaUI)}.{nameof(PullOnce)} called.");

            if (isPulling || gachaManager == null || !gachaManager.TryPullSingle(out var result))
            {
                Debug.LogWarning("Pull 1x failed: already pulling, no active banner, insufficient gems, or GachaManager unavailable.");
                return;
            }

            StartCoroutine(PlayPullSequence(new List<GachaPullResult> { result }));
        }

        public void PullTen()
        {
            Debug.Log($"{nameof(GachaUI)}.{nameof(PullTen)} called.");

            if (isPulling || gachaManager == null || !gachaManager.TryPullTen(out var results))
            {
                Debug.LogWarning("Pull 10x failed: already pulling, no active banner, insufficient gems, or GachaManager unavailable.");
                return;
            }

            StartCoroutine(PlayPullSequence(results));
        }

        public void RefreshPityDisplay()
        {
            if (pityText == null || gachaManager == null) return;
            pityText.text = $"Pity: {gachaManager.Pity.PullsSinceLastFiveStar}/{GameConstants.HardPityThreshold} | 5* rate: {gachaManager.GetCurrentFiveStarRate():F2}%";
        }

        private IEnumerator PlayPullSequence(List<GachaPullResult> results)
        {
            isPulling = true;
            if (summaryPanel != null) summaryPanel.SetActive(false);
            ClearResultCards();

            if (bannerAnimator != null)
            {
                bannerAnimator.SetTrigger(OpenTrigger);
                yield return new WaitForSeconds(revealDelaySeconds);
            }

            foreach (var result in results)
            {
                string name = result.pulledCharacter != null ? result.pulledCharacter.characterName : "Unknown";
                Debug.Log($"Pulled {name} ({result.rarity}){(result.isNew ? " NEW" : " (duplicate)")}");
                SpawnResultCard(result);
                yield return new WaitForSeconds(revealDelaySeconds);
            }

            if (resultText != null)
                resultText.text = string.Join("\n", results.ConvertAll(r =>
                    $"{(r.pulledCharacter != null ? r.pulledCharacter.characterName : "?")} ({r.rarity})"));

            RefreshPityDisplay();
            if (summaryPanel != null) summaryPanel.SetActive(true);
            isPulling = false;
        }

        private void SpawnResultCard(GachaPullResult result)
        {
            if (resultCardPrefab == null || resultCardContainer == null) return;

            var go = Instantiate(resultCardPrefab, resultCardContainer);
            var label = go.GetComponentInChildren<Text>();
            if (label != null && result.pulledCharacter != null)
                label.text = $"{result.pulledCharacter.characterName} ({result.rarity}){(result.isNew ? " NEW" : "")}";
        }

        private void ClearResultCards()
        {
            if (resultCardContainer == null) return;
            for (int i = resultCardContainer.childCount - 1; i >= 0; i--)
                Destroy(resultCardContainer.GetChild(i).gameObject);
        }
    }
}
