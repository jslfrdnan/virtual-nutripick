using System.Collections.Generic;
using System.Text;
using NutriPick.Content;
using NutriPick.Nutrition;
using UnityEngine;

namespace NutriPick.Quests
{
    /// <summary>
    /// Builds a short, actionable hint when serving is blocked by a quest task.
    /// The recommended plate is always a combination from the level pool that
    /// passes every task, preferring foods already on the player's plate.
    /// </summary>
    public static class QuestHintBuilder
    {
        public static string Build(
            QuestEvaluation evaluation,
            LevelDefinition level,
            IReadOnlyList<FoodData> plateFoods)
        {
            if (evaluation == null || evaluation.quest == null || level == null || level.foodPool == null)
                return string.Empty;

            QuestTaskStatus failing = null;
            foreach (var task in evaluation.tasks)
            {
                if (task != null && !task.passed)
                {
                    failing = task;
                    break;
                }
            }

            if (failing == null)
                return string.Empty;

            int required = Mathf.Clamp(level.minItems, 1, Mathf.Max(1, CountDistinct(level.foodPool)));
            var combo = FindCombo(evaluation, level, plateFoods, failing, required);
            var builder = new StringBuilder();
            builder.Append("Task belum lengkap: ").Append(failing.label);

            if (failing.kind == QuestTaskKind.NutrientMinimum)
            {
                builder.Append(" (")
                    .Append(QuestEvaluator.FormatAmount(failing.current, failing.nutrient))
                    .Append(" / ")
                    .Append(QuestEvaluator.FormatAmount(failing.target, failing.nutrient))
                    .Append(")");
            }

            builder.Append('\n');

            if (combo != null && combo.Count == required)
            {
                builder.Append("Coba: ");
                for (int i = 0; i < combo.Count; i++)
                {
                    if (i > 0)
                        builder.Append(" + ");
                    builder.Append(combo[i].displayName);
                }
            }
            else
            {
                builder.Append(BuildFallback(failing, level, plateFoods));
            }

            return builder.ToString();
        }

        private static List<FoodData> FindCombo(
            QuestEvaluation evaluation,
            LevelDefinition level,
            IReadOnlyList<FoodData> plateFoods,
            QuestTaskStatus failing,
            int required)
        {
            var pool = new List<FoodData>();
            var seen = new HashSet<string>();
            foreach (var food in level.foodPool)
            {
                if (food != null && seen.Add(food.id))
                    pool.Add(food);
            }

            if (pool.Count < required)
                return null;

            var plateCounts = CountFoods(plateFoods);
            var combo = new List<FoodData>(required);
            List<FoodData> best = null;
            int bestOverlap = -1;
            float bestValue = float.MinValue;

            SearchCombinations(
                pool, required, 0, combo, evaluation, failing, plateCounts,
                ref best, ref bestOverlap, ref bestValue);

            return best;
        }

        private static void SearchCombinations(
            List<FoodData> pool,
            int required,
            int start,
            List<FoodData> combo,
            QuestEvaluation evaluation,
            QuestTaskStatus failing,
            Dictionary<string, int> plateCounts,
            ref List<FoodData> best,
            ref int bestOverlap,
            ref float bestValue)
        {
            if (combo.Count == required)
            {
                var candidate = QuestEvaluator.Evaluate(combo, evaluation.quest, evaluation.levelIndex);
                if (!candidate.AllPassed)
                    return;

                int overlap = 0;
                foreach (var food in combo)
                {
                    if (plateCounts.ContainsKey(food.id))
                        overlap++;
                }

                float value = failing.kind == QuestTaskKind.NutrientMinimum
                    ? candidate.totals.Get(failing.nutrient)
                    : 0f;

                if (overlap > bestOverlap || (overlap == bestOverlap && value > bestValue))
                {
                    bestOverlap = overlap;
                    bestValue = value;
                    best = new List<FoodData>(combo);
                }

                return;
            }

            int remaining = required - combo.Count;
            for (int i = start; i <= pool.Count - remaining; i++)
            {
                combo.Add(pool[i]);
                SearchCombinations(
                    pool, required, i + 1, combo, evaluation, failing, plateCounts,
                    ref best, ref bestOverlap, ref bestValue);
                combo.RemoveAt(combo.Count - 1);
            }
        }

        private static string BuildFallback(
            QuestTaskStatus task,
            LevelDefinition level,
            IReadOnlyList<FoodData> plateFoods)
        {
            if (task.kind == QuestTaskKind.Composition)
            {
                var examples = FindCategoryFoods(level.foodPool, plateFoods, task.category, 2);
                if (examples.Count == 0)
                    return "Butuh " + task.label + ".";

                var builder = new StringBuilder("Butuh ").Append(task.label).Append(": ");
                AppendNames(builder, examples);
                return builder.ToString();
            }

            var foods = FindNutrientFoods(level.foodPool, plateFoods, task.nutrient, 2);
            if (foods.Count == 0)
                return "Tambah makanan kaya " + task.label.ToLowerInvariant() + ".";

            var hint = new StringBuilder("Tambah: ");
            for (int i = 0; i < foods.Count; i++)
            {
                if (i > 0)
                    hint.Append(" atau ");
                hint.Append(foods[i].displayName)
                    .Append(" (")
                    .Append(QuestEvaluator.FormatAmount(GetNutrient(foods[i], task.nutrient), task.nutrient))
                    .Append(")");
            }

            return hint.ToString();
        }

        private static List<FoodData> FindNutrientFoods(
            IReadOnlyList<FoodData> pool,
            IReadOnlyList<FoodData> plateFoods,
            NutrientType nutrient,
            int count)
        {
            var plateCounts = CountFoods(plateFoods);
            var candidates = new List<FoodData>();
            var backups = new List<FoodData>();

            foreach (var food in pool)
            {
                if (food == null)
                    continue;

                if (plateCounts.ContainsKey(food.id))
                    backups.Add(food);
                else
                    candidates.Add(food);
            }

            candidates.Sort((left, right) =>
                GetNutrient(right, nutrient).CompareTo(GetNutrient(left, nutrient)));

            if (candidates.Count < count)
            {
                backups.Sort((left, right) =>
                    GetNutrient(right, nutrient).CompareTo(GetNutrient(left, nutrient)));

                foreach (var food in backups)
                {
                    if (candidates.Count >= count)
                        break;
                    candidates.Add(food);
                }
            }

            if (candidates.Count > count)
                candidates.RemoveRange(count, candidates.Count - count);

            return candidates;
        }

        private static List<FoodData> FindCategoryFoods(
            IReadOnlyList<FoodData> pool,
            IReadOnlyList<FoodData> plateFoods,
            FoodCompositionCategory category,
            int count)
        {
            var plateCounts = CountFoods(plateFoods);
            var candidates = new List<FoodData>();
            var backups = new List<FoodData>();

            foreach (var food in pool)
            {
                if (food == null || !food.HasCategory(category))
                    continue;

                if (plateCounts.ContainsKey(food.id))
                    backups.Add(food);
                else
                    candidates.Add(food);
            }

            foreach (var food in backups)
            {
                if (candidates.Count >= count)
                    break;
                candidates.Add(food);
            }

            if (candidates.Count > count)
                candidates.RemoveRange(count, candidates.Count - count);

            return candidates;
        }

        private static int CountDistinct(IReadOnlyList<FoodData> foods)
        {
            var ids = new HashSet<string>();
            foreach (var food in foods)
            {
                if (food != null)
                    ids.Add(food.id);
            }

            return ids.Count;
        }

        private static void AppendNames(StringBuilder builder, IReadOnlyList<FoodData> foods)
        {
            for (int i = 0; i < foods.Count; i++)
            {
                if (i > 0)
                    builder.Append(" atau ");
                builder.Append(foods[i].displayName);
            }
        }

        private static Dictionary<string, int> CountFoods(IReadOnlyList<FoodData> foods)
        {
            var counts = new Dictionary<string, int>();
            if (foods == null)
                return counts;

            foreach (var food in foods)
            {
                if (food == null || string.IsNullOrEmpty(food.id))
                    continue;

                counts.TryGetValue(food.id, out var count);
                counts[food.id] = count + 1;
            }

            return counts;
        }

        private static float GetNutrient(FoodData food, NutrientType nutrient)
        {
            if (food == null)
                return 0f;

            switch (nutrient)
            {
                case NutrientType.Calories:
                    return food.calories;
                case NutrientType.Protein:
                    return food.protein;
                case NutrientType.Carbohydrate:
                    return food.carbs;
                case NutrientType.Fat:
                    return food.fat;
                case NutrientType.Fiber:
                    return food.fiber;
                case NutrientType.AddedSugar:
                    return food.addedSugar;
                case NutrientType.Sodium:
                    return food.sodium;
                case NutrientType.VitaminA:
                    return food.vitaminA;
                case NutrientType.VitaminC:
                    return food.vitaminC;
                case NutrientType.VitaminD:
                    return food.vitaminD;
                case NutrientType.Calcium:
                    return food.calcium;
                case NutrientType.Iron:
                    return food.iron;
                default:
                    return 0f;
            }
        }
    }
}
