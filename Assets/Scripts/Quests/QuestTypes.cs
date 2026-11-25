using System;
using NutriPick.Nutrition;

namespace NutriPick.Quests
{
    public enum QuestGroup
    {
        Anak,
        Dewasa,
        Lansia
    }

    public enum NutrientType
    {
        Calories,
        Protein,
        Carbohydrate,
        Fat,
        Fiber,
        AddedSugar,
        Sodium,
        VitaminA,
        VitaminC,
        VitaminD,
        Calcium,
        Iron
    }

    [Serializable]
    public class AkgProfile
    {
        public float calories = 2000f;
        public float protein = 60f;
        public float carbohydrate = 300f;
        public float fat = 60f;
        public float fiber = 28f;
        public float addedSugar = 25f;
        public float sodium = 1500f;
        public float vitaminA = 600f;
        public float vitaminC = 75f;
        public float vitaminD = 15f;
        public float calcium = 1000f;
        public float iron = 12f;

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

    [Serializable]
    public class QuestNutrientTarget
    {
        public NutrientType nutrient;
        public float minimum;
    }

    [Serializable]
    public class QuestLevelTasks
    {
        public string label = "Sarapan";
        public FoodCompositionCategory requiredA = FoodCompositionCategory.Protein;
        public FoodCompositionCategory requiredB = FoodCompositionCategory.Fruit;
        public QuestNutrientTarget targetA = new QuestNutrientTarget();
        public QuestNutrientTarget targetB = new QuestNutrientTarget();
    }
}
