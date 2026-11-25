using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NutriPick.Nutrition;
using NutriPick.Quests;

namespace NutriPick.Tests
{
    public class QuestEvaluatorTests
    {
        private QuestDefinition quest;
        private readonly List<FoodData> foods = new List<FoodData>();

        [SetUp]
        public void SetUp()
        {
            quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.id = "test";
            quest.displayName = "Test";
            quest.ageRange = "7-9";
            quest.dailyAkg = new AkgProfile
            {
                protein = 45f,
                fiber = 26f,
                sodium = 1200f,
                addedSugar = 25f,
                fat = 60f,
                calcium = 1000f
            };
            quest.levels = new[]
            {
                new QuestLevelTasks
                {
                    label = "Sarapan",
                    requiredA = FoodCompositionCategory.Protein,
                    requiredB = FoodCompositionCategory.Fruit,
                    targetA = new QuestNutrientTarget { nutrient = NutrientType.Protein, minimum = 15f },
                    targetB = new QuestNutrientTarget { nutrient = NutrientType.Calcium, minimum = 330f }
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(quest);
            foreach (var food in foods)
                Object.DestroyImmediate(food);
            foods.Clear();
        }

        private FoodData Food(
            string id,
            FoodCompositionCategory composition,
            float protein,
            float calcium,
            float sodium = 0f)
        {
            var food = ScriptableObject.CreateInstance<FoodData>();
            food.id = id;
            food.displayName = id;
            food.composition = composition;
            food.protein = protein;
            food.calcium = calcium;
            food.sodium = sodium;
            foods.Add(food);
            return food;
        }

        [Test]
        public void BuildsFourTasksAndPassesWhenCompositionAndNutrientsMatch()
        {
            var plate = new[]
            {
                Food("tahu", FoodCompositionCategory.Protein, 8f, 350f),
                Food("pisang", FoodCompositionCategory.Fruit, 1.3f, 6f),
                Food("tempe", FoodCompositionCategory.Protein, 20f, 111f),
                Food("brokoli", FoodCompositionCategory.Vegetable, 2.8f, 47f)
            };

            var evaluation = QuestEvaluator.Evaluate(plate, quest, 1);
            Assert.AreEqual(4, evaluation.tasks.Count);
            Assert.IsTrue(evaluation.AllPassed, evaluation.MissingReason);
            Assert.AreEqual(32.1f, evaluation.totals.protein, 0.01f);
            Assert.AreEqual(514f, evaluation.totals.calcium, 0.01f);
        }

        [Test]
        public void MissingFruitFailsComposition()
        {
            var plate = new[]
            {
                Food("tahu", FoodCompositionCategory.Protein, 8f, 350f),
                Food("tempe", FoodCompositionCategory.Protein, 20f, 111f),
                Food("brokoli", FoodCompositionCategory.Vegetable, 2.8f, 47f),
                Food("wortel", FoodCompositionCategory.Vegetable, 0.9f, 33f)
            };

            var evaluation = QuestEvaluator.Evaluate(plate, quest, 1);
            Assert.IsFalse(evaluation.AllPassed);
            StringAssert.Contains("BUAH", evaluation.MissingReason);
        }

        [Test]
        public void LowCalciumFailsNutrientTask()
        {
            var plate = new[]
            {
                Food("telur", FoodCompositionCategory.Protein, 6.3f, 25f),
                Food("pisang", FoodCompositionCategory.Fruit, 1.3f, 6f),
                Food("tempe", FoodCompositionCategory.Protein, 20f, 111f),
                Food("brokoli", FoodCompositionCategory.Vegetable, 2.8f, 47f)
            };

            var evaluation = QuestEvaluator.Evaluate(plate, quest, 1);
            Assert.IsFalse(evaluation.AllPassed);
            StringAssert.Contains("KALSIUM", evaluation.MissingReason);
        }

        [Test]
        public void OverLimitSodiumAddsMealWarning()
        {
            var plate = new[]
            {
                Food("asin", FoodCompositionCategory.Protein, 20f, 350f, 500f),
                Food("pisang", FoodCompositionCategory.Fruit, 1.3f, 6f)
            };

            var evaluation = QuestEvaluator.Evaluate(plate, quest, 1);
            Assert.AreEqual(1, evaluation.warnings.Length);
            StringAssert.Contains("NATRIUM", evaluation.warnings[0]);
        }

        [Test]
        public void CampaignWarningUsesDailyLimit()
        {
            var totals = new NutritionTotals { sodium = 1300f };
            var warnings = QuestEvaluator.CampaignWarnings(totals, quest);
            Assert.AreEqual(1, warnings.Length);
            StringAssert.Contains("NATRIUM", warnings[0]);
        }
    }
}
