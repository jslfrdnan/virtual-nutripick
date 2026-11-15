using System.Collections.Generic;
using UnityEngine;

namespace NutriPick.Nutrition
{
    public static class MealEvaluator
    {
        private static readonly FoodCompositionCategory[] CoreCategories =
        {
            FoodCompositionCategory.Carbohydrate,
            FoodCompositionCategory.Protein,
            FoodCompositionCategory.Vegetable
        };

        private static readonly FoodCompositionCategory[] VarietyCategories =
        {
            FoodCompositionCategory.Vegetable,
            FoodCompositionCategory.Protein,
            FoodCompositionCategory.Carbohydrate,
            FoodCompositionCategory.Fruit,
            FoodCompositionCategory.Drink
        };

        public static MealResult Evaluate(
            IReadOnlyList<FoodData> foods,
            NutritionRules rules,
            int minItems = 0,
            bool requiresDrink = false)
        {
            if (foods == null || foods.Count == 0 || rules == null)
            {
                return new MealResult
                {
                    score = 50,
                    band = ScoreBand.Neutral,
                    bandLabel = "Belum dinilai",
                    healthyCount = 0,
                    lessHealthyCount = 0,
                    itemScores = new List<FoodScore>(),
                    strengths = System.Array.Empty<string>(),
                    improvement = string.Empty,
                    isIncomplete = false,
                    warnings = System.Array.Empty<string>()
                };
            }

            var result = new MealResult();
            float totalHealth = 0f;
            float totalFiber = 0f;
            float totalProtein = 0f;
            float totalSugar = 0f;
            float totalSodium = 0f;
            float totalCalories = 0f;
            int ultraProcessedCount = 0;
            int friedCount = 0;
            FoodCompositionCategory covered = FoodCompositionCategory.None;

            foreach (var food in foods)
            {
                if (food == null)
                    continue;

                var label = FoodClassifier.Resolve(food, rules);
                float health = FoodClassifier.HealthValue(food, rules);
                totalHealth += health;

                if (label == FoodHealthLabel.Healthy)
                    result.healthyCount++;
                else
                    result.lessHealthyCount++;

                totalFiber += food.fiber;
                totalProtein += food.protein;
                totalSugar += food.addedSugar;
                totalSodium += food.sodium;
                totalCalories += food.calories;
                if (food.processing == ProcessingLevel.UltraProcessed)
                    ultraProcessedCount++;
                if (food.isFried)
                    friedCount++;
                covered |= food.composition;

                result.itemScores.Add(new FoodScore
                {
                    foodId = food.id,
                    displayName = food.displayName,
                    healthLabel = label,
                    healthValue = health
                });
            }

            int count = result.itemScores.Count;
            if (count == 0)
                return Evaluate(null, rules, minItems, requiresDrink);

            result.averageItemHealth = totalHealth / count;
            result.compositionCovered = covered;

            int coreCovered = 0;
            foreach (var category in CoreCategories)
            {
                if ((covered & category) != 0)
                    coreCovered++;
            }

            int distinct = 0;
            foreach (var category in VarietyCategories)
            {
                if ((covered & category) != 0)
                    distinct++;
            }

            result.compositionScore = Mathf.Clamp01(
                rules.coreWeight * (coreCovered / 3f)
                + rules.varietyWeight * (distinct / 5f)
                - rules.missingCorePenalty * (3 - coreCovered));

            result.score = Mathf.RoundToInt(100f * Mathf.Clamp01(
                rules.itemWeight * result.averageItemHealth
                + rules.compositionWeight * result.compositionScore));

            var band = rules.GetBand(result.score);
            result.band = band.band;
            result.bandLabel = band.label;

            bool hasDrink = (covered & FoodCompositionCategory.Drink) != 0;
            result.isIncomplete = count < minItems || (requiresDrink && !hasDrink);

            var warnings = new List<string>();
            if (result.isIncomplete)
                warnings.Add("Piring belum lengkap");
            result.warnings = warnings.ToArray();

            result.strengths = BuildStrengths(result, totalFiber, totalProtein, totalSugar, totalSodium, distinct, ultraProcessedCount, covered);
            result.improvement = BuildImprovement(result, totalFiber, totalProtein, totalSugar, totalSodium, totalCalories, ultraProcessedCount, friedCount, covered);
            result.dimensions = BuildDimensions(result, totalFiber, totalProtein, totalSugar, totalSodium, totalCalories, ultraProcessedCount, rules);

            return result;
        }

        private static string[] BuildStrengths(
            MealResult result,
            float fiber,
            float protein,
            float sugar,
            float sodium,
            int distinct,
            int ultraProcessedCount,
            FoodCompositionCategory covered)
        {
            var candidates = new List<string>();

            if ((covered & FoodCompositionCategory.Vegetable) != 0)
                candidates.Add("Sayur hadir di piring.");
            if (protein >= 20f)
                candidates.Add("Protein cukup untuk kebutuhan harian.");
            if (fiber >= 6f)
                candidates.Add("Serat cukup untuk pencernaan.");
            if (sugar <= 5f)
                candidates.Add("Gula tambahan rendah.");
            if (sodium <= 600f)
                candidates.Add("Natrium terkendali.");
            if (distinct >= 4)
                candidates.Add("Piring beragam.");
            if (ultraProcessedCount == 0)
                candidates.Add("Pilihan minim proses.");
            if ((covered & FoodCompositionCategory.Fruit) != 0)
                candidates.Add("Ada buah segar.");

            while (candidates.Count < 2)
            {
                candidates.Add(candidates.Count == 0
                    ? "Kamu sudah menyusun piring sendiri."
                    : "Kamu bisa mengulang level untuk mencoba kombinasi lain.");
            }

            return new[] { candidates[0], candidates[1] };
        }

        private static string BuildImprovement(
            MealResult result,
            float fiber,
            float protein,
            float sugar,
            float sodium,
            float calories,
            int ultraProcessedCount,
            int friedCount,
            FoodCompositionCategory covered)
        {
            if ((covered & FoodCompositionCategory.Vegetable) == 0)
                return "Tambahkan sayur.";
            if (sugar > 10f)
                return "Kurangi gula tambahan.";
            if (sodium > 800f)
                return "Kurangi natrium.";
            if (protein < 15f)
                return "Tambahkan protein.";
            if (fiber < 5f)
                return "Tambahkan serat.";
            if (friedCount > 0 || ultraProcessedCount > 0)
                return "Kurangi gorengan atau makanan ultra-proses.";
            if ((covered & FoodCompositionCategory.Carbohydrate) == 0)
                return "Tambahkan karbohidrat.";
            if ((covered & FoodCompositionCategory.Fruit) == 0 && result.score < 70)
                return "Tambahkan buah.";
            if (calories > 800f)
                return "Jaga porsi makan.";
            return "Pertahankan kombinasi ini dan sesuaikan porsinya.";
        }

        private static HealthDimensions BuildDimensions(
            MealResult result,
            float fiber,
            float protein,
            float sugar,
            float sodium,
            float calories,
            int ultraProcessedCount,
            NutritionRules rules)
        {
            var dimensions = new HealthDimensions();

            if (calories < rules.energyLow)
            {
                dimensions.energy.rating = DimensionRating.Rendah;
                dimensions.energy.label = "Rendah";
                dimensions.energy.summary = "Porsimu ringan; tambahkan sumber energi bila kamu aktif.";
            }
            else if (calories > rules.energyHigh || sugar > 15f)
            {
                dimensions.energy.rating = DimensionRating.Berlebih;
                dimensions.energy.label = "Berlebih";
                dimensions.energy.summary = "Energi cepat naik, tetapi bisa turun lagi.";
            }
            else
            {
                dimensions.energy.rating = DimensionRating.Baik;
                dimensions.energy.label = "Baik";
                dimensions.energy.summary = "Energimu cukup stabil sampai sore.";
            }

            if (protein >= rules.focusProteinMin && fiber >= rules.focusFiberMin && sugar <= rules.focusSugarMax)
            {
                dimensions.focus.rating = DimensionRating.Baik;
                dimensions.focus.label = "Baik";
                dimensions.focus.summary = "Konsentrasi lebih stabil di jam belajar.";
            }
            else if (protein < 12f || fiber < 3f)
            {
                dimensions.focus.rating = DimensionRating.Rendah;
                dimensions.focus.label = "Rendah";
                dimensions.focus.summary = "Protein dan serat masih rendah untuk fokus panjang.";
            }
            else
            {
                dimensions.focus.rating = DimensionRating.Cukup;
                dimensions.focus.label = "Cukup";
                dimensions.focus.summary = "Cukup, tetapi bisa lebih stabil dengan protein dan serat.";
            }

            if (fiber >= rules.longTermFiberMin && sodium <= rules.longTermSodiumMax && ultraProcessedCount <= rules.longTermUltraMax)
            {
                dimensions.longTerm.rating = DimensionRating.Baik;
                dimensions.longTerm.label = "Baik";
                dimensions.longTerm.summary = "Kebiasaan ini lebih ramah untuk tekanan darah.";
            }
            else if (sodium > rules.longTermSodiumHigh || ultraProcessedCount >= rules.longTermUltraHigh || sugar > rules.longTermSugarHigh)
            {
                dimensions.longTerm.rating = DimensionRating.PerluPerhatian;
                dimensions.longTerm.label = "Perlu perhatian";
                dimensions.longTerm.summary = "Natrium dan makanan ultra-proses masih tinggi.";
            }
            else
            {
                dimensions.longTerm.rating = DimensionRating.Cukup;
                dimensions.longTerm.label = "Cukup";
                dimensions.longTerm.summary = "Sudah ada dasar; kurangi natrium untuk jangka panjang.";
            }

            return dimensions;
        }
    }
}
