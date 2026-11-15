using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Tests
{
    public class MealEvaluatorTests
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

        private static FoodData Food(
            string id,
            FoodCompositionCategory composition,
            int calories,
            float protein,
            float fiber,
            float sugar,
            float sodium,
            ProcessingLevel processing = ProcessingLevel.Minimal,
            bool fried = false,
            bool drink = false)
        {
            var food = ScriptableObject.CreateInstance<FoodData>();
            food.id = id;
            food.displayName = id;
            food.description = "Test";
            food.composition = composition;
            food.processing = processing;
            food.isFried = fried;
            food.isDrink = drink;
            food.calories = calories;
            food.protein = protein;
            food.carbs = 10f;
            food.fat = 2f;
            food.fiber = fiber;
            food.addedSugar = sugar;
            food.sodium = sodium;
            food.portionLabel = "100 g";
            return food;
        }

        private static void DestroyAll(params FoodData[] foods)
        {
            foreach (var food in foods)
            {
                if (food != null)
                    Object.DestroyImmediate(food);
            }
        }

        [Test]
        public void EmptyPlateIsNeutralFifty()
        {
            var result = MealEvaluator.Evaluate(new List<FoodData>(), rules);
            Assert.AreEqual(50, result.score);
            Assert.AreEqual(ScoreBand.Neutral, result.band);
            Assert.AreEqual("Belum dinilai", result.bandLabel);
        }

        [Test]
        public void BalancedPlateScoresAtLeastSeventy()
        {
            var rice = Food("rice", FoodCompositionCategory.Carbohydrate, 195, 4f, 0.6f, 0f, 2f);
            var egg = Food("egg", FoodCompositionCategory.Protein, 78, 6.3f, 0f, 0f, 62f);
            var tempeh = Food("tempeh", FoodCompositionCategory.Protein, 100, 9f, 1.5f, 0f, 120f, ProcessingLevel.Processed);
            var broccoli = Food("broccoli", FoodCompositionCategory.Vegetable, 34, 2.8f, 2.6f, 0f, 33f);

            var result = MealEvaluator.Evaluate(new List<FoodData> { rice, egg, tempeh, broccoli }, rules);
            Assert.GreaterOrEqual(result.score, 70);
            Assert.AreEqual(ScoreBand.Balanced, result.band);
            Assert.AreEqual(2, result.strengths.Length);
            Assert.IsFalse(string.IsNullOrEmpty(result.improvement));
            DestroyAll(rice, egg, tempeh, broccoli);
        }

        [Test]
        public void FriedPlateScoresBelowFifty()
        {
            var egg = Food("fried_egg", FoodCompositionCategory.Protein, 110, 6.5f, 0f, 0f, 170f, ProcessingLevel.Processed, true);
            var noodles = Food("noodles", FoodCompositionCategory.Carbohydrate | FoodCompositionCategory.Snack, 330, 8f, 2f, 3f, 800f, ProcessingLevel.UltraProcessed, true);
            var crackers = Food("crackers", FoodCompositionCategory.Snack, 150, 1f, 0.3f, 0f, 300f, ProcessingLevel.UltraProcessed, true);

            var result = MealEvaluator.Evaluate(new List<FoodData> { egg, noodles, crackers }, rules);
            Assert.Less(result.score, 50);
            Assert.AreEqual(ScoreBand.NeedsImprovement, result.band);
            DestroyAll(egg, noodles, crackers);
        }

        [Test]
        public void ThreeBananasScoreBelowForty()
        {
            var banana = Food("banana", FoodCompositionCategory.Fruit, 105, 1.3f, 3.1f, 0f, 1f);
            var result = MealEvaluator.Evaluate(new List<FoodData> { banana, banana, banana }, rules);
            Assert.Less(result.score, 40);
            Object.DestroyImmediate(banana);
        }

        [Test]
        public void ScoreIsClampedAndOrderIndependent()
        {
            var rice = Food("rice", FoodCompositionCategory.Carbohydrate, 195, 4f, 0.6f, 0f, 2f);
            var broccoli = Food("broccoli", FoodCompositionCategory.Vegetable, 34, 2.8f, 2.6f, 0f, 33f);
            var water = Food("water", FoodCompositionCategory.Drink, 0, 0f, 0f, 0f, 0f, ProcessingLevel.Minimal, false, true);

            var first = MealEvaluator.Evaluate(new List<FoodData> { rice, broccoli, water }, rules);
            var second = MealEvaluator.Evaluate(new List<FoodData> { water, broccoli, rice }, rules);
            Assert.AreEqual(first.score, second.score);
            Assert.That(first.score, Is.InRange(0, 100));
            DestroyAll(rice, broccoli, water);
        }

        [Test]
        public void LevelThreeWithoutDrinkIsIncomplete()
        {
            var rice = Food("rice", FoodCompositionCategory.Carbohydrate, 195, 4f, 0.6f, 0f, 2f);
            var broccoli = Food("broccoli", FoodCompositionCategory.Vegetable, 34, 2.8f, 2.6f, 0f, 33f);
            var egg = Food("egg", FoodCompositionCategory.Protein, 78, 6.3f, 0f, 0f, 62f);

            var result = MealEvaluator.Evaluate(new List<FoodData> { rice, broccoli, egg }, rules, 4, true);
            Assert.IsTrue(result.isIncomplete);
            Assert.Greater(result.warnings.Length, 0);
            DestroyAll(rice, broccoli, egg);
        }

        [Test]
        public void LevelThreeWithWaterIsComplete()
        {
            var rice = Food("rice", FoodCompositionCategory.Carbohydrate, 195, 4f, 0.6f, 0f, 2f);
            var broccoli = Food("broccoli", FoodCompositionCategory.Vegetable, 34, 2.8f, 2.6f, 0f, 33f);
            var egg = Food("egg", FoodCompositionCategory.Protein, 78, 6.3f, 0f, 0f, 62f);
            var water = Food("water", FoodCompositionCategory.Drink, 0, 0f, 0f, 0f, 0f, ProcessingLevel.Minimal, false, true);

            var result = MealEvaluator.Evaluate(new List<FoodData> { rice, broccoli, egg, water }, rules, 4, true);
            Assert.IsFalse(result.isIncomplete);
            DestroyAll(rice, broccoli, egg, water);
        }
    }
}
