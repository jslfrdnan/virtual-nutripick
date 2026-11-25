using System;
using System.Collections.Generic;
using NutriPick.Nutrition;
using UnityEngine;

namespace NutriPick.Quests
{
    [Serializable]
    public class NutritionTotals
    {
        public float calories;
        public float protein;
        public float carbohydrate;
        public float fat;
        public float fiber;
        public float addedSugar;
        public float sodium;
        public float vitaminA;
        public float vitaminC;
        public float vitaminD;
        public float calcium;
        public float iron;

        public static NutritionTotals From(IReadOnlyList<FoodData> foods)
        {
            var totals = new NutritionTotals();
            if (foods == null)
                return totals;

            foreach (var food in foods)
                totals.AddFood(food);
            return totals;
        }

        public void AddFood(FoodData food)
        {
            if (food == null)
                return;

            calories += food.calories;
            protein += food.protein;
            carbohydrate += food.carbs;
            fat += food.fat;
            fiber += food.fiber;
            addedSugar += food.addedSugar;
            sodium += food.sodium;
            vitaminA += food.vitaminA;
            vitaminC += food.vitaminC;
            vitaminD += food.vitaminD;
            calcium += food.calcium;
            iron += food.iron;
        }

        public void Add(NutritionTotals other)
        {
            if (other == null)
                return;

            calories += other.calories;
            protein += other.protein;
            carbohydrate += other.carbohydrate;
            fat += other.fat;
            fiber += other.fiber;
            addedSugar += other.addedSugar;
            sodium += other.sodium;
            vitaminA += other.vitaminA;
            vitaminC += other.vitaminC;
            vitaminD += other.vitaminD;
            calcium += other.calcium;
            iron += other.iron;
        }

        public float Get(NutrientType type)
        {
            return type switch
            {
                NutrientType.Calories => calories,
                NutrientType.Protein => protein,
                NutrientType.Carbohydrate => carbohydrate,
                NutrientType.Fat => fat,
                NutrientType.Fiber => fiber,
                NutrientType.AddedSugar => addedSugar,
                NutrientType.Sodium => sodium,
                NutrientType.VitaminA => vitaminA,
                NutrientType.VitaminC => vitaminC,
                NutrientType.VitaminD => vitaminD,
                NutrientType.Calcium => calcium,
                NutrientType.Iron => iron,
                _ => 0f
            };
        }
    }
}
