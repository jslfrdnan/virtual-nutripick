using System.Collections.Generic;

namespace NutriPick.Nutrition
{
    public readonly struct MealRequirementState
    {
        public readonly int count;
        public readonly bool carbohydrate;
        public readonly bool protein;
        public readonly bool vegetable;
        public readonly bool extra;

        public MealRequirementState(int count, bool carbohydrate, bool protein, bool vegetable, bool extra)
        {
            this.count = count;
            this.carbohydrate = carbohydrate;
            this.protein = protein;
            this.vegetable = vegetable;
            this.extra = extra;
        }

        public bool IsComplete => count >= MealRequirement.RequiredItems
            && carbohydrate
            && protein
            && vegetable
            && extra;

        public string MissingReason
        {
            get
            {
                if (count < MealRequirement.RequiredItems)
                    return $"Tambahkan {MealRequirement.RequiredItems - count} makanan lagi";
                if (!carbohydrate)
                    return "Tambahkan 1 karbohidrat";
                if (!protein)
                    return "Tambahkan 1 protein";
                if (!vegetable)
                    return "Tambahkan 1 sayur";
                if (!extra)
                    return "Tambahkan 1 makanan lagi";
                return string.Empty;
            }
        }

        public static MealRequirementState Empty => new MealRequirementState(0, false, false, false, false);
    }

    public static class MealRequirement
    {
        public const int RequiredItems = 4;

        public static MealRequirementState Evaluate(IReadOnlyList<FoodData> foods)
        {
            int count = 0;
            bool carbohydrate = false;
            bool protein = false;
            bool vegetable = false;

            if (foods != null)
            {
                foreach (var food in foods)
                {
                    if (food == null)
                        continue;

                    count++;
                    if (food.HasCategory(FoodCompositionCategory.Carbohydrate))
                        carbohydrate = true;
                    if (food.HasCategory(FoodCompositionCategory.Protein))
                        protein = true;
                    if (food.HasCategory(FoodCompositionCategory.Vegetable))
                        vegetable = true;
                }
            }

            return new MealRequirementState(count, carbohydrate, protein, vegetable, count >= RequiredItems);
        }

        public static bool TryGetFriedFoods(IReadOnlyList<FoodData> foods, out List<string> names)
        {
            names = new List<string>();
            if (foods == null)
                return false;

            foreach (var food in foods)
            {
                if (food == null || !food.isFried)
                    continue;

                names.Add(string.IsNullOrWhiteSpace(food.displayName) ? food.id : food.displayName);
            }

            return names.Count > 0;
        }
    }
}
