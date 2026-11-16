using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Tests
{
    public class Scene1DataTests
    {
        private NutritionRules rules;
        private readonly List<FoodData> created = new List<FoodData>();

        [SetUp]
        public void SetUp()
        {
            rules = ScriptableObject.CreateInstance<NutritionRules>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var food in created)
            {
                if (food != null)
                    Object.DestroyImmediate(food);
            }
            created.Clear();

            if (rules != null)
                Object.DestroyImmediate(rules);
        }

        private FoodData Food(
            string id,
            FoodCompositionCategory composition,
            int calories,
            float protein,
            float fiber,
            float sugar,
            float sodium,
            ProteinType proteinType = ProteinType.Nabati)
        {
            var food = ScriptableObject.CreateInstance<FoodData>();
            food.id = id;
            food.displayName = id;
            food.description = "Test";
            food.composition = composition;
            food.processing = ProcessingLevel.Minimal;
            food.isFried = false;
            food.isDrink = false;
            food.calories = calories;
            food.protein = protein;
            food.carbs = 10f;
            food.fat = 2f;
            food.fiber = fiber;
            food.addedSugar = sugar;
            food.sodium = sodium;
            food.portionLabel = "100 g";
            food.hasProteinType = true;
            food.proteinType = proteinType;
            created.Add(food);
            return food;
        }

        [Test]
        public void ProteinTypeCanBeAssigned()
        {
            var food = Food("tempe", FoodCompositionCategory.Protein, 100, 9f, 1.5f, 0f, 120f, ProteinType.Nabati);
            Assert.IsTrue(food.hasProteinType);
            Assert.AreEqual(ProteinType.Nabati, food.proteinType);
        }

        [Test]
        public void MealEvaluatorProducesThreePointsPerPanel()
        {
            var rice = Food("rice", FoodCompositionCategory.Carbohydrate, 195, 4f, 0.6f, 0f, 2f);
            var egg = Food("egg", FoodCompositionCategory.Protein, 78, 6.3f, 0f, 0f, 62f, ProteinType.Hewani);
            var broccoli = Food("broccoli", FoodCompositionCategory.Vegetable, 34, 2.8f, 2.6f, 0f, 33f);
            var banana = Food("banana", FoodCompositionCategory.Fruit, 105, 1.3f, 3.1f, 0f, 1f);

            var result = MealEvaluator.Evaluate(
                new List<FoodData> { rice, egg, broccoli, banana },
                rules,
                3,
                false);

            Assert.AreEqual(3, result.positivePoints.Length);
            Assert.AreEqual(3, result.improvementPoints.Length);
            foreach (var point in result.positivePoints)
                Assert.IsFalse(string.IsNullOrWhiteSpace(point));
            foreach (var point in result.improvementPoints)
                Assert.IsFalse(string.IsNullOrWhiteSpace(point));
            Assert.IsTrue(result.statusBadge == "LEBIH SEHAT" || result.statusBadge == "PERLU DIUBAH");
            Assert.AreEqual(result.band == ScoreBand.Balanced, result.isBalanced);
        }
    }
}
