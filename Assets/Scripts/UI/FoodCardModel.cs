using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.UI
{
    public class FoodCardModel
    {
        public FoodData food;
        public string foodId;
        public string displayName;
        public Sprite icon;
        public string categoryLabel;
        public bool isHealthy;

        public static FoodCardModel From(FoodData food, NutritionRules rules)
        {
            if (food == null)
                return new FoodCardModel
                {
                    foodId = string.Empty,
                    displayName = "—",
                    categoryLabel = "—"
                };

            return new FoodCardModel
            {
                food = food,
                foodId = food.id,
                displayName = string.IsNullOrWhiteSpace(food.displayName) ? food.id : food.displayName,
                icon = food.icon,
                categoryLabel = CategoryLabel(food.composition),
                isHealthy = FoodClassifier.Resolve(food, rules) == FoodHealthLabel.Healthy
            };
        }

        public static string CategoryLabel(FoodCompositionCategory composition)
        {
            switch (composition)
            {
                case FoodCompositionCategory.Carbohydrate:
                    return "KARBOHIDRAT";
                case FoodCompositionCategory.Protein:
                    return "PROTEIN";
                case FoodCompositionCategory.Vegetable:
                    return "SAYUR";
                case FoodCompositionCategory.Fruit:
                    return "BUAH";
                case FoodCompositionCategory.Snack:
                    return "CAMILAN";
                case FoodCompositionCategory.Drink:
                    return "MINUMAN";
                default:
                    return "CAMPURAN";
            }
        }
    }
}
