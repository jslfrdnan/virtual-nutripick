using System;
using System.Collections.Generic;
using System.Globalization;
using NutriPick.Nutrition;

namespace NutriPick.Quests
{
    public enum QuestTaskKind
    {
        Composition,
        NutrientMinimum
    }

    [Serializable]
    public class QuestTaskStatus
    {
        public QuestTaskKind kind;
        public string label;
        public bool passed;
        public float current;
        public float target;
        public FoodCompositionCategory category;
        public NutrientType nutrient;

        public string ProgressLabel()
        {
            if (kind == QuestTaskKind.Composition)
                return label + (passed ? " [x]" : " [ ]");

            return label + " " + QuestEvaluator.FormatAmount(current, nutrient) + "/" +
                QuestEvaluator.FormatAmount(target, nutrient) + (passed ? " [x]" : " [ ]");
        }
    }

    [Serializable]
    public class QuestEvaluation
    {
        public QuestDefinition quest;
        public int levelIndex;
        public List<QuestTaskStatus> tasks = new List<QuestTaskStatus>();
        public NutritionTotals totals = new NutritionTotals();
        public string[] warnings = Array.Empty<string>();

        public bool AllPassed
        {
            get
            {
                foreach (var task in tasks)
                {
                    if (!task.passed)
                        return false;
                }

                return tasks.Count > 0;
            }
        }

        public string MissingReason
        {
            get
            {
                foreach (var task in tasks)
                {
                    if (!task.passed)
                        return "Task belum lengkap: " + task.label;
                }

                return string.Empty;
            }
        }
    }

    public static class QuestEvaluator
    {
        private static readonly NutrientType[] WarningNutrients =
        {
            NutrientType.Sodium,
            NutrientType.AddedSugar,
            NutrientType.Fat
        };

        public static QuestEvaluation Evaluate(
            IReadOnlyList<FoodData> foods,
            QuestDefinition quest,
            int levelIndex)
        {
            var evaluation = new QuestEvaluation
            {
                quest = quest,
                levelIndex = levelIndex,
                totals = NutritionTotals.From(foods)
            };

            if (quest == null || quest.levels == null || quest.levels.Length == 0)
                return evaluation;

            int index = Math.Clamp(levelIndex - 1, 0, quest.levels.Length - 1);
            var level = quest.levels[index];
            if (level == null)
                return evaluation;

            evaluation.tasks.Add(CompositionTask(foods, level.requiredA));
            evaluation.tasks.Add(CompositionTask(foods, level.requiredB));
            evaluation.tasks.Add(NutrientTask(evaluation.totals, level.targetA));
            evaluation.tasks.Add(NutrientTask(evaluation.totals, level.targetB));
            evaluation.warnings = MealWarnings(evaluation.totals, quest.dailyAkg);
            return evaluation;
        }

        public static string[] CampaignWarnings(NutritionTotals totals, QuestDefinition quest)
        {
            if (totals == null || quest == null)
                return Array.Empty<string>();

            return BuildWarnings(totals, quest.dailyAkg, 1f, "hari ini");
        }

        public static string CategoryLabel(FoodCompositionCategory category)
        {
            return category switch
            {
                FoodCompositionCategory.Carbohydrate => "KARBO",
                FoodCompositionCategory.Protein => "PROTEIN",
                FoodCompositionCategory.Vegetable => "SAYUR",
                FoodCompositionCategory.Fruit => "BUAH",
                FoodCompositionCategory.Snack => "CAMILAN",
                FoodCompositionCategory.Drink => "MINUMAN",
                _ => "MAKANAN"
            };
        }

        public static string NutrientLabel(NutrientType nutrient)
        {
            return nutrient switch
            {
                NutrientType.Calories => "ENERGI",
                NutrientType.Protein => "PROTEIN",
                NutrientType.Carbohydrate => "KARBO",
                NutrientType.Fat => "LEMAK",
                NutrientType.Fiber => "SERAT",
                NutrientType.AddedSugar => "GULA",
                NutrientType.Sodium => "NATRIUM",
                NutrientType.VitaminA => "VIT A",
                NutrientType.VitaminC => "VIT C",
                NutrientType.VitaminD => "VIT D",
                NutrientType.Calcium => "KALSIUM",
                NutrientType.Iron => "BESI",
                _ => "GIZI"
            };
        }

        public static string Unit(NutrientType nutrient)
        {
            return nutrient switch
            {
                NutrientType.Calories => "kkal",
                NutrientType.VitaminA => "µg",
                NutrientType.VitaminD => "µg",
                _ => nutrient == NutrientType.VitaminC || nutrient == NutrientType.Sodium ||
                    nutrient == NutrientType.Calcium || nutrient == NutrientType.Iron
                    ? "mg"
                    : "g"
            };
        }

        public static string FormatValue(NutrientType nutrient, float value)
        {
            string unit = Unit(nutrient);
            string pattern = nutrient == NutrientType.Calories || unit == "mg" ? "0" : "0.#";
            return value.ToString(pattern, CultureInfo.InvariantCulture) + " " + unit;
        }

        public static string FormatAmount(float value, NutrientType nutrient = NutrientType.Protein)
        {
            return FormatValue(nutrient, value).Replace('.', ',');
        }

        private static QuestTaskStatus CompositionTask(
            IReadOnlyList<FoodData> foods,
            FoodCompositionCategory category)
        {
            bool found = false;
            if (foods != null && category != FoodCompositionCategory.None)
            {
                foreach (var food in foods)
                {
                    if (food != null && food.HasCategory(category))
                    {
                        found = true;
                        break;
                    }
                }
            }

            return new QuestTaskStatus
            {
                kind = QuestTaskKind.Composition,
                label = CategoryLabel(category),
                passed = found,
                category = category,
                current = found ? 1f : 0f,
                target = 1f
            };
        }

        private static QuestTaskStatus NutrientTask(NutritionTotals totals, QuestNutrientTarget target)
        {
            if (target == null)
                return new QuestTaskStatus { kind = QuestTaskKind.NutrientMinimum, label = "—", passed = true };

            float current = totals.Get(target.nutrient);
            return new QuestTaskStatus
            {
                kind = QuestTaskKind.NutrientMinimum,
                label = NutrientLabel(target.nutrient),
                nutrient = target.nutrient,
                current = current,
                target = target.minimum,
                passed = current >= target.minimum
            };
        }

        private static string[] MealWarnings(NutritionTotals totals, AkgProfile daily)
        {
            return BuildWarnings(totals, daily, 1f / 3f, "makan ini");
        }

        private static string[] BuildWarnings(
            NutritionTotals totals,
            AkgProfile daily,
            float fraction,
            string period)
        {
            if (daily == null)
                return Array.Empty<string>();

            var warnings = new List<string>();
            foreach (var nutrient in WarningNutrients)
            {
                float limit = daily.Get(nutrient) * fraction;
                float value = totals.Get(nutrient);
                if (limit <= 0f || value <= limit)
                    continue;

                warnings.Add($"{NutrientLabel(nutrient)} {FormatAmount(value, nutrient)} melebihi batas {period} ({FormatAmount(limit, nutrient)}).");
            }

            return warnings.ToArray();
        }
    }
}
