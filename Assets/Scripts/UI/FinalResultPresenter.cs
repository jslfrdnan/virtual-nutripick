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
        public TMP_Text inputHint;
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

            EnsureFocusHighlight(retryButton);
            EnsureFocusHighlight(nextButton);
            ConfigureNavigation();
            SetButtonLabel(nextButton, level != null && level.isFinalLevel ? "MAIN LAGI" : "NEXT LEVEL");
            EnsureInputHint();
            SelectInitialButton();
        }

        public void Hide()
        {
            if (EventSystem.current != null)
            {
                var selected = EventSystem.current.currentSelectedGameObject;
                if ((retryButton != null && selected == retryButton.gameObject) ||
                    (nextButton != null && selected == nextButton.gameObject))
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            if (root != null)
                root.SetActive(false);
        }

        private void SelectInitialButton()
        {
            if (EventSystem.current == null || retryButton == null)
                return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        }

        private static void EnsureFocusHighlight(Button button)
        {
            if (button == null)
                return;

            if (button.GetComponent<SelectableFocusHighlight>() == null)
                button.gameObject.AddComponent<SelectableFocusHighlight>();
        }

        private void ConfigureNavigation()
        {
            if (retryButton == null)
                return;

            bool hasNext = nextButton != null && nextButton.gameObject.activeSelf;

            var retryNavigation = retryButton.navigation;
            retryNavigation.mode = Navigation.Mode.Explicit;
            retryNavigation.selectOnLeft = null;
            retryNavigation.selectOnRight = hasNext ? nextButton : null;
            retryNavigation.selectOnUp = null;
            retryNavigation.selectOnDown = null;
            retryButton.navigation = retryNavigation;

            if (!hasNext)
                return;

            var nextNavigation = nextButton.navigation;
            nextNavigation.mode = Navigation.Mode.Explicit;
            nextNavigation.selectOnLeft = retryButton;
            nextNavigation.selectOnRight = null;
            nextNavigation.selectOnUp = null;
            nextNavigation.selectOnDown = null;
            nextButton.navigation = nextNavigation;
        }

        private void EnsureInputHint()
        {
            const string hint = "D-pad / left stick: pilih · A / Enter: konfirmasi";

            if (root == null)
                return;

            if (inputHint == null)
            {
                var existing = root.transform.Find("InputHint");
                if (existing != null)
                    inputHint = existing.GetComponent<TMP_Text>();
            }

            if (inputHint == null)
            {
                var go = new GameObject("InputHint", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(root.transform, false);

                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, -455f);
                rect.sizeDelta = new Vector2(900f, 24f);

                var text = go.GetComponent<TextMeshProUGUI>();
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 16f;
                text.color = new Color(0.78f, 0.82f, 0.84f, 1f);
                text.raycastTarget = false;
                if (titleText != null)
                    text.font = titleText.font;

                inputHint = text;
            }

            inputHint.text = hint;
            inputHint.gameObject.SetActive(true);
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button == null)
                return;

            var label = button.transform.Find("Label");
            if (label == null)
                return;

            var text = label.GetComponent<TMP_Text>();
            if (text != null)
                text.text = value;
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
