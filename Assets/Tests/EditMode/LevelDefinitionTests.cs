using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NutriPick.Content;
using NutriPick.Nutrition;

namespace NutriPick.Tests
{
    public class LevelDefinitionTests
    {
        private readonly List<FoodData> created = new List<FoodData>();

        [TearDown]
        public void TearDown()
        {
            foreach (var food in created)
            {
                if (food != null)
                    Object.DestroyImmediate(food);
            }
            created.Clear();
        }

        private FoodData Food(string id, FoodCompositionCategory composition, bool drink = false)
        {
            var food = ScriptableObject.CreateInstance<FoodData>();
            food.id = id;
            food.displayName = id;
            food.description = "Test";
            food.composition = composition;
            food.processing = ProcessingLevel.Minimal;
            food.isDrink = drink;
            food.calories = 100;
            food.protein = 5f;
            food.fiber = 3f;
            food.portionLabel = "100 g";
            created.Add(food);
            return food;
        }

        private LevelDefinition Level(params FoodData[] foods)
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.levelId = "test_level";
            level.displayName = "Test Level";
            level.sceneName = "Level01_Warteg";
            level.nextSceneName = "Level02_Padang";
            level.levelIndex = 1;
            level.settingDescription = "Test";
            level.foodPool = foods;
            level.minItems = 3;
            level.timeLimitSeconds = 180f;
            level.graceSeconds = 30f;
            return level;
        }

        [Test]
        public void ValidLevelPasses()
        {
            var level = Level(
                Food("a", FoodCompositionCategory.Vegetable),
                Food("b", FoodCompositionCategory.Protein),
                Food("c", FoodCompositionCategory.Carbohydrate),
                Food("d", FoodCompositionCategory.Fruit),
                Food("e", FoodCompositionCategory.Snack),
                Food("f", FoodCompositionCategory.Snack));
            Assert.IsTrue(level.IsValid(out var error), error);
            Object.DestroyImmediate(level);
        }

        [Test]
        public void PoolMustHaveSixToEightFoods()
        {
            var level = Level(
                Food("a", FoodCompositionCategory.Vegetable),
                Food("b", FoodCompositionCategory.Protein));
            Assert.IsFalse(level.IsValid(out _));
            Object.DestroyImmediate(level);
        }

        [Test]
        public void LevelThreeRequiresDrink()
        {
            var level = Level(
                Food("a", FoodCompositionCategory.Vegetable),
                Food("b", FoodCompositionCategory.Protein),
                Food("c", FoodCompositionCategory.Carbohydrate),
                Food("d", FoodCompositionCategory.Fruit),
                Food("e", FoodCompositionCategory.Snack),
                Food("f", FoodCompositionCategory.Snack));
            level.levelIndex = 3;
            level.isFinalLevel = true;
            level.requiresDrink = true;
            level.minimumStands = 3;
            Assert.IsFalse(level.IsValid(out _));
            Object.DestroyImmediate(level);
        }
    }
}
