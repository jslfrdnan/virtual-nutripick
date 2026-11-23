using NUnit.Framework;
using UnityEngine;
using NutriPick.Nutrition;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class Scene1CardModelTests
    {
        private NutritionRules rules;
        private FoodData food;

        [SetUp]
        public void SetUp()
        {
            rules = ScriptableObject.CreateInstance<NutritionRules>();
            food = ScriptableObject.CreateInstance<FoodData>();
            food.id = "rendang";
            food.displayName = "Rendang";
            food.description = "Test";
            food.composition = FoodCompositionCategory.Protein;
            food.processing = ProcessingLevel.Processed;
            food.calories = 250;
            food.protein = 15f;
            food.carbs = 5f;
            food.fat = 19f;
            food.fiber = 1f;
            food.addedSugar = 0f;
            food.sodium = 300f;
            food.portionLabel = "100 g";
            food.hasProteinType = true;
            food.proteinType = ProteinType.Hewani;
        }

        [TearDown]
        public void TearDown()
        {
            if (food != null) Object.DestroyImmediate(food);
            if (rules != null) Object.DestroyImmediate(rules);
        }

        [Test]
        public void MapsFoodValuesToCardText()
        {
            var model = FoodCardModel.From(food, rules);
            Assert.AreEqual("rendang", model.foodId);
            Assert.AreEqual("Rendang", model.displayName);
            Assert.AreEqual("PROTEIN", model.categoryLabel);
        }

        [Test]
        public void MapsCarbohydrateCategory()
        {
            food.composition = FoodCompositionCategory.Carbohydrate;
            var model = FoodCardModel.From(food, rules);
            Assert.AreEqual("KARBOHIDRAT", model.categoryLabel);
        }
    }
}
