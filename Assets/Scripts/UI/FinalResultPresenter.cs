using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Quests;

namespace NutriPick.UI
{
    public class FinalResultPresenter : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text titleText;
        public TMP_Text subtitleText;
        public TMP_Text taskText;
        public TMP_Text nutritionText;
        public TMP_Text warningText;
        public TMP_Text closingText;
        public TMP_Text disclaimerText;
        public TMP_Text inputHint;
        public Button retryButton;
        public Button nextButton;

        public void Show(QuestEvaluation evaluation, LevelDefinition level, Action onRetry, Action onNext)
        {
            if (root != null)
                root.SetActive(true);

            bool finalLevel = level != null && level.isFinalLevel;
            if (titleText != null)
                titleText.text = finalLevel ? "HASIL AKHIR" : "HASIL LEVEL";
            if (subtitleText != null)
                subtitleText.text = Subtitle(evaluation, level);

            if (taskText != null)
                taskText.text = BuildTasks(evaluation);
            if (nutritionText != null)
                nutritionText.text = BuildNutrition(evaluation);
            if (warningText != null)
            {
                bool hasWarning = evaluation != null && evaluation.warnings != null && evaluation.warnings.Length > 0;
                warningText.text = hasWarning
                    ? string.Join("\n", evaluation.warnings)
                    : "Tidak ada batas makan yang terlampaui.";
                warningText.color = hasWarning
                    ? new Color(0.95f, 0.42f, 0.36f)
                    : new Color(0.30f, 0.69f, 0.31f);
            }

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
            SetButtonLabel(nextButton, finalLevel ? "LIHAT RINGKASAN" : "NEXT LEVEL");
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

        private static string Subtitle(QuestEvaluation evaluation, LevelDefinition level)
        {
            string questName = evaluation != null && evaluation.quest != null
                ? evaluation.quest.displayName
                : "Quest";
            string levelName = level != null ? level.displayName : "Level";
            return questName + " · " + levelName;
        }

        private static string BuildTasks(QuestEvaluation evaluation)
        {
            if (evaluation == null || evaluation.tasks.Count == 0)
                return "Tidak ada task.";

            var builder = new StringBuilder();
            builder.Append("TASK");
            foreach (var task in evaluation.tasks)
            {
                builder.Append('\n');
                builder.Append(task.ProgressLabel());
            }

            return builder.ToString();
        }

        private static string BuildNutrition(QuestEvaluation evaluation)
        {
            if (evaluation == null || evaluation.totals == null)
                return "—";

            var totals = evaluation.totals;
            var builder = new StringBuilder();
            builder.Append("GIZI PIRING");
            Append(builder, NutrientType.Calories, totals.Get(NutrientType.Calories));
            Append(builder, NutrientType.Protein, totals.Get(NutrientType.Protein));
            Append(builder, NutrientType.Carbohydrate, totals.Get(NutrientType.Carbohydrate));
            Append(builder, NutrientType.Fat, totals.Get(NutrientType.Fat));
            Append(builder, NutrientType.Fiber, totals.Get(NutrientType.Fiber));
            Append(builder, NutrientType.Sodium, totals.Get(NutrientType.Sodium));
            Append(builder, NutrientType.AddedSugar, totals.Get(NutrientType.AddedSugar));
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, NutrientType nutrient, float value)
        {
            builder.Append('\n');
            builder.Append(QuestEvaluator.NutrientLabel(nutrient));
            builder.Append(": ");
            builder.Append(QuestEvaluator.FormatValue(nutrient, value));
        }

        private void SelectInitialButton()
        {
            if (EventSystem.current == null)
                return;

            // After a successful serve, continue is the expected default action.
            // Retry remains reachable by navigating left.
            var target = nextButton != null && nextButton.gameObject.activeSelf
                ? nextButton
                : retryButton;

            if (target == null)
                return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target.gameObject);
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
    }
}
