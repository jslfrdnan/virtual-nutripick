using NUnit.Framework;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Tests
{
    public class FoodClassifierTests
    {
        private NutritionRules rules;

        [SetUp]
        public void SetUp()
        {
            rules = ScriptableObject.CreateInstance<NutritionRules>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(rules);
        }

        private static FoodData MakeFood()
        {
            var food = ScriptableObject.CreateInstance<FoodData>();
            food.id = "test_food";
            food.displayName = "Test";
            food.description = "Test food";
            food.composition = FoodCompositionCategory.Vegetable;
            food.processing = ProcessingLevel.Minimal;
            food.calories = 100;
            food.protein = 5f;
            food.carbs = 10f;
            food.fat = 1f;
            food.fiber = 3f;
            food.addedSugar = 0f;
            food.sodium = 100f;
            food.portionLabel = "100 g";
            return food;
        }

        [Test]
        public void FriedFoodIsLessHealthy()
        {
            var food = MakeFood();
            food.isFried = true;
            Assert.AreEqual(FoodHealthLabel.LessHealthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }

        [Test]
        public void HighSugarIsLessHealthy()
        {
            var food = MakeFood();
            food.addedSugar = 11f;
            Assert.AreEqual(FoodHealthLabel.LessHealthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }

        [Test]
        public void HighSodiumIsLessHealthy()
        {
            var food = MakeFood();
            food.sodium = 501f;
            Assert.AreEqual(FoodHealthLabel.LessHealthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }

        [Test]
        public void UltraProcessedIsLessHealthy()
        {
            var food = MakeFood();
            food.processing = ProcessingLevel.UltraProcessed;
            Assert.AreEqual(FoodHealthLabel.LessHealthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }

        [Test]
        public void SugaryDrinkIsLessHealthy()
        {
            var food = MakeFood();
            food.composition = FoodCompositionCategory.Drink;
            food.isDrink = true;
            food.addedSugar = 6f;
            Assert.AreEqual(FoodHealthLabel.LessHealthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }

        [Test]
        public void PlainVegetableIsHealthy()
        {
            var food = MakeFood();
            Assert.AreEqual(FoodHealthLabel.Healthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }

        [Test]
        public void ForceLessHealthyOverrideWins()
        {
            var food = MakeFood();
            food.healthyOverride = HealthyOverride.ForceLessHealthy;
            Assert.AreEqual(FoodHealthLabel.LessHealthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }

        [Test]
        public void ForceHealthyOverrideWins()
        {
            var food = MakeFood();
            food.isFried = true;
            food.healthyOverride = HealthyOverride.ForceHealthy;
            Assert.AreEqual(FoodHealthLabel.Healthy, FoodClassifier.Resolve(food, rules));
            Object.DestroyImmediate(food);
        }
    }
}
