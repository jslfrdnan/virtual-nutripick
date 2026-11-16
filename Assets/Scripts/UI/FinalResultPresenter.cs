using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class FinalResultPresenter : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text titleText;
        public TMP_Text balancedTitleText;
        public TMP_Text lessBalancedTitleText;
        public TMP_Text[] balancedPointTexts = new TMP_Text[3];
        public TMP_Text[] lessBalancedPointTexts = new TMP_Text[3];
        public TMP_Text balancedBadgeText;
        public TMP_Text lessBalancedBadgeText;
        public Image balancedPanelBackground;
        public Image lessBalancedPanelBackground;
        public Image balancedBadgeBackground;
        public Image lessBalancedBadgeBackground;
        public TMP_Text closingText;
        public TMP_Text disclaimerText;
        public Button retryButton;
        public Button nextButton;

        public void Show(MealResult result, LevelDefinition level, Action onRetry, Action onNext)
        {
            if (root != null)
                root.SetActive(true);
            if (titleText != null)
                titleText.text = "FINAL RESULT";
            if (balancedTitleText != null)
                balancedTitleText.text = "Pilihan yang mendukung tubuh seimbang";
            if (lessBalancedTitleText != null)
                lessBalancedTitleText.text = "Pilihan kurang seimbang";

            FillPoints(balancedPointTexts, result != null ? result.positivePoints : null, new[]
            {
                "Kamu sudah menyusun piring sendiri.",
                "Kamu bisa mengulang level untuk mencoba kombinasi lain.",
                "Setiap pilihan adalah langkah belajar."
            });
            FillPoints(lessBalancedPointTexts, result != null ? result.improvementPoints : null, new[]
            {
                "Coba variasi makanan lain.",
                "Perhatikan porsi berikutnya.",
                "Tambahkan sayur atau buah jika memungkinkan."
            });

            if (balancedBadgeText != null) balancedBadgeText.text = "LEBIH SEHAT";
            if (lessBalancedBadgeText != null) lessBalancedBadgeText.text = "PERLU DIUBAH";
            if (balancedBadgeBackground != null)
                balancedBadgeBackground.color = new Color(0.30f, 0.69f, 0.31f);
            if (lessBalancedBadgeBackground != null)
                lessBalancedBadgeBackground.color = new Color(0.95f, 0.42f, 0.36f);

            bool balanced = result != null && result.isBalanced;
            if (balancedPanelBackground != null)
                balancedPanelBackground.color = balanced
                    ? new Color(0.88f, 0.96f, 0.88f, 1f)
                    : new Color(0.88f, 0.96f, 0.88f, 0.45f);
            if (lessBalancedPanelBackground != null)
                lessBalancedPanelBackground.color = balanced
                    ? new Color(0.99f, 0.90f, 0.88f, 0.45f)
                    : new Color(0.99f, 0.90f, 0.88f, 1f);

            if (closingText != null)
                closingText.text = "PILIHAN KECIL, DAMPAK BESAR!";
            if (disclaimerText != null)
                disclaimerText.text = "Game edukasi, bukan nasihat medis. Nilai gizi adalah perkiraan.";

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

        private static void FillPoints(TMP_Text[] targets, string[] source, string[] fallback)
        {
            if (targets == null)
                return;

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    continue;

                string value = source != null && i < source.Length && !string.IsNullOrWhiteSpace(source[i])
                    ? source[i]
                    : (i < fallback.Length ? fallback[i] : "—");
                targets[i].text = value;
            }
        }
    }
}
