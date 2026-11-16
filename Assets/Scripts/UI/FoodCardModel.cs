using UnityEngine;
using NutriPick.Nutrition;
using NutriPick.Utilities;

namespace NutriPick.UI
{
    public class FoodCardModel
    {
        public FoodData food;
        public string foodId;
        public string displayName;
        public Sprite icon;
        public string proteinTypeLabel;
        public string caloriesText;
        public string proteinText;
        public string carbsText;
        public string fatText;
        public bool isHealthy;

        public static FoodCardModel From(FoodData food, NutritionRules rules)
        {
            if (food == null)
                return new FoodCardModel
                {
                    foodId = string.Empty,
                    displayName = "—",
                    proteinTypeLabel = "—",
                    caloriesText = "—",
                    proteinText = "—",
                    carbsText = "—",
                    fatText = "—"
                };

            return new FoodCardModel
            {
                food = food,
                foodId = food.id,
                displayName = string.IsNullOrWhiteSpace(food.displayName) ? food.id : food.displayName,
                icon = food.icon,
                proteinTypeLabel = food.hasProteinType
                    ? (food.proteinType == ProteinType.Hewani ? "Hewani" : "Nabati")
                    : "—",
                caloriesText = NutritionFormat.Kcal(food.calories),
                proteinText = NutritionFormat.Grams(food.protein),
                carbsText = NutritionFormat.Grams(food.carbs),
                fatText = NutritionFormat.Grams(food.fat),
                isHealthy = FoodClassifier.Resolve(food, rules) == FoodHealthLabel.Healthy
            };
        }
    }
}
