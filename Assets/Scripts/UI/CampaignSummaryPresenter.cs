using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NutriPick.Flow;
using NutriPick.Quests;

namespace NutriPick.UI
{
    public class CampaignSummaryPresenter : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text titleText;
        public TMP_Text subtitleText;
        public TMP_Text levelsText;
        public TMP_Text totalsText;
        public TMP_Text warningText;
        public Button restartButton;
        public TMP_Text inputHint;

        private static readonly NutrientType[] ReportNutrients =
        {
            NutrientType.Calories,
            NutrientType.Protein,
            NutrientType.Carbohydrate,
            NutrientType.Fat,
            NutrientType.Fiber,
            NutrientType.AddedSugar,
            NutrientType.Sodium,
            NutrientType.VitaminA,
            NutrientType.VitaminC,
            NutrientType.VitaminD,
            NutrientType.Calcium,
            NutrientType.Iron
        };

        public void Show(QuestDefinition quest, LevelResult[] results, NutritionTotals totals, Action onRestart)
        {
            if (root != null)
                root.SetActive(true);

            if (titleText != null)
                titleText.text = "RINGKASAN KAMPANYE";
            if (subtitleText != null)
            {
                subtitleText.text = quest != null
                    ? quest.displayName + " · Usia " + quest.ageRange
                    : "Kampanye selesai";
            }

            if (levelsText != null)
                levelsText.text = BuildLevels(quest, results);
            if (totalsText != null)
                totalsText.text = BuildTotals(quest, totals);
            if (warningText != null)
            {
                var warnings = QuestEvaluator.CampaignWarnings(totals, quest);
                warningText.text = warnings.Length > 0
                    ? string.Join("\n", warnings)
                    : "Total harian masih dalam batas.";
                warningText.color = warnings.Length > 0
                    ? new Color(0.95f, 0.42f, 0.36f)
                    : new Color(0.30f, 0.69f, 0.31f);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => onRestart?.Invoke());
                EnsureFocusHighlight(restartButton);
            }

            if (EventSystem.current != null && restartButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(restartButton.gameObject);
            }

            EnsureInputHint();
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private static string BuildLevels(QuestDefinition quest, LevelResult[] results)
        {
            var builder = new StringBuilder();
            builder.Append("HASIL PER LEVEL");
            if (results == null)
                return builder.ToString();

            foreach (var result in results)
            {
                if (result == null)
                    continue;

                string label = quest != null && quest.levels != null &&
                    result.levelIndex >= 1 && result.levelIndex <= quest.levels.Length
                    ? quest.levels[result.levelIndex - 1].label
                    : "Level " + result.levelIndex;

                int passed = 0;
                int total = 0;
                if (result.evaluation != null)
                {
                    total = result.evaluation.tasks.Count;
                    foreach (var task in result.evaluation.tasks)
                    {
                        if (task.passed)
                            passed++;
                    }
                }

                builder.Append('\n');
                builder.Append(label.ToUpperInvariant());
                builder.Append(": ").Append(passed).Append('/').Append(total).Append(" task");
            }

            return builder.ToString();
        }

        private static string BuildTotals(QuestDefinition quest, NutritionTotals totals)
        {
            var builder = new StringBuilder();
            builder.Append("TOTAL HARIAN");
            if (totals == null)
                return builder.ToString();

            foreach (var nutrient in ReportNutrients)
            {
                float value = totals.Get(nutrient);
                builder.Append('\n');
                builder.Append(QuestEvaluator.NutrientLabel(nutrient));
                builder.Append(": ");
                builder.Append(QuestEvaluator.FormatValue(nutrient, value));

                if (quest != null && quest.dailyAkg != null)
                {
                    float akg = quest.dailyAkg.Get(nutrient);
                    if (akg > 0f)
                        builder.Append(" (").Append(Mathf.RoundToInt(value / akg * 100f)).Append("% AKG)");
                }
            }

            return builder.ToString();
        }

        private static void EnsureFocusHighlight(Button button)
        {
            if (button != null && button.GetComponent<SelectableFocusHighlight>() == null)
                button.gameObject.AddComponent<SelectableFocusHighlight>();
        }

        private void EnsureInputHint()
        {
            const string hint = "A / Enter: main lagi";

            if (root == null)
                return;

            if (inputHint == null)
            {
                var existing = root.transform.Find("InputHint");
                if (existing != null)
                    inputHint = existing.GetComponent<TMP_Text>();
            }

            if (inputHint != null)
                inputHint.text = hint;
        }
    }
}
