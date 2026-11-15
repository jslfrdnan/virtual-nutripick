using UnityEngine;

namespace NutriPick.Nutrition
{
    public static class FoodClassifier
    {
        public static FoodHealthLabel Resolve(FoodData food, NutritionRules rules)
        {
            if (food == null || rules == null)
                return FoodHealthLabel.LessHealthy;

            if (food.healthyOverride == HealthyOverride.ForceHealthy)
                return FoodHealthLabel.Healthy;

            if (food.healthyOverride == HealthyOverride.ForceLessHealthy)
                return FoodHealthLabel.LessHealthy;

            bool isSnackOnly = food.HasCategory(FoodCompositionCategory.Snack)
                && !food.HasCategory(FoodCompositionCategory.Vegetable)
                && !food.HasCategory(FoodCompositionCategory.Fruit);

            bool isSugaryDrink = food.isDrink && food.addedSugar > rules.healthyDrinkSugarMax;

            if (food.addedSugar > rules.healthySugarMax ||
                food.sodium > rules.healthySodiumMax ||
                food.processing == ProcessingLevel.UltraProcessed ||
                food.isFried ||
                isSnackOnly ||
                isSugaryDrink)
            {
                return FoodHealthLabel.LessHealthy;
            }

            return FoodHealthLabel.Healthy;
        }

        public static float HealthValue(FoodData food, NutritionRules rules)
        {
            if (food == null || rules == null)
                return 0f;

            float processingPenalty = food.processing switch
            {
                ProcessingLevel.Minimal => 0f,
                ProcessingLevel.Processed => 0.5f,
                ProcessingLevel.UltraProcessed => 1f,
                _ => 0f
            };

            float friedPenalty = food.isFried ? 1f : 0f;
            float drinkBonus = food.isDrink && food.addedSugar <= 0f ? 1f : 0f;

            float value = 0.50f
                + rules.fiberWeight * Mathf.Clamp01(food.fiber / rules.fiberTargetGrams)
                + rules.proteinWeight * Mathf.Clamp01(food.protein / rules.proteinTargetGrams)
                - rules.sugarWeight * Mathf.Clamp01(food.addedSugar / rules.sugarTargetGrams)
                - rules.sodiumWeight * Mathf.Clamp01(food.sodium / rules.sodiumTargetMg)
                - rules.processingWeight * processingPenalty
                - rules.friedWeight * friedPenalty
                + rules.unsweetenedDrinkBonus * drinkBonus;

            return Mathf.Clamp(value, rules.itemHealthMin, rules.itemHealthMax);
        }
    }
}
