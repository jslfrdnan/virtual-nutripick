using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class HealthReportPresenter : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text titleText;
        public TMP_Text scoreText;
        public TMP_Text bandText;
        public TMP_Text strengthsText;
        public TMP_Text improvementText;
        public TMP_Text dimensionsText;
        public TMP_Text warningText;
        public TMP_Text disclaimerText;
        public Button retryButton;
        public Button nextButton;

        public void Show(MealResult result, LevelDefinition level, Action onRetry, Action onNext)
        {
            if (root != null)
                root.SetActive(true);
            if (titleText != null)
                titleText.text = "Laporan " + (level != null ? level.displayName : "Level");
            if (scoreText != null)
                scoreText.text = result.score.ToString();
            if (bandText != null)
                bandText.text = result.bandLabel;
            if (strengthsText != null)
            {
                string first = result.strengths != null && result.strengths.Length > 0 ? result.strengths[0] : "Kamu sudah menyusun piring.";
                string second = result.strengths != null && result.strengths.Length > 1 ? result.strengths[1] : "Kamu bisa mencoba kombinasi lain.";
                strengthsText.text = "✓ " + first + "\n✓ " + second;
            }
            if (improvementText != null)
                improvementText.text = "→ " + result.improvement;
            if (dimensionsText != null)
            {
                dimensionsText.text =
                    $"Energi: {result.dimensions.energy.label} — {result.dimensions.energy.summary}\n" +
                    $"Fokus: {result.dimensions.focus.label} — {result.dimensions.focus.summary}\n" +
                    $"Kesehatan jangka panjang: {result.dimensions.longTerm.label} — {result.dimensions.longTerm.summary}";
            }
            if (warningText != null)
            {
                warningText.gameObject.SetActive(result.isIncomplete);
                warningText.text = "Piring belum lengkap, tetapi tetap dinilai.";
            }
            if (disclaimerText != null)
                disclaimerText.text = "Ini adalah game edukasi, bukan nasihat medis. Nilai gizi adalah perkiraan.";

            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners();
                retryButton.onClick.AddListener(() => onRetry?.Invoke());
            }
            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(() => onNext?.Invoke());
                nextButton.gameObject.SetActive(onNext != null);
            }

            if (EventSystem.current != null && retryButton != null)
                EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
