using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
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
            level.nextSceneName = "Level02_Warteg";
            level.levelIndex = 1;
            level.settingDescription = "Test";
            level.foodPool = foods;
            level.minItems = 5;
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
        public void PoolMustHaveAtLeastSixFoods()
        {
            var level = Level(
                Food("a", FoodCompositionCategory.Vegetable),
                Food("b", FoodCompositionCategory.Protein));
            Assert.IsFalse(level.IsValid(out _));
            Object.DestroyImmediate(level);
        }

        [Test]
        public void CampaignChainIsWartegThenBazaar()
        {
            var level1 = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_MainVR.asset");
            var level2 = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset");
            var level3 = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_Level03_Bazaar.asset");

            Assert.IsNotNull(level1);
            Assert.IsNotNull(level2);
            Assert.IsNotNull(level3);
            Assert.AreEqual("MainVR", level1.sceneName);
            Assert.AreEqual("Level02_Warteg", level1.nextSceneName);
            Assert.AreEqual("Level02_Warteg", level2.sceneName);
            Assert.AreEqual("Level03_Bazaar", level2.nextSceneName);
            Assert.AreEqual("Level03_Bazaar", level3.sceneName);
            Assert.IsTrue(level3.isFinalLevel);
        }

        [Test]
        public void AllCampaignLevelsAreDrinkFree()
        {
            var paths = new[]
            {
                "Assets/ScriptableObjects/Levels/LD_MainVR.asset",
                "Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset",
                "Assets/ScriptableObjects/Levels/LD_Level03_Bazaar.asset"
            };

            foreach (var path in paths)
            {
                var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                Assert.IsNotNull(level, path);
                Assert.IsFalse(level.requiresDrink, $"{path} must not require a drink.");
                Assert.IsNotNull(level.foodPool, $"{path} has no food pool.");

                foreach (var food in level.foodPool)
                {
                    Assert.IsNotNull(food, $"{path} contains a null food.");
                    Assert.IsFalse(food.isDrink, $"{path} still contains drink '{food.id}'.");
                }
            }
        }
    }
}
