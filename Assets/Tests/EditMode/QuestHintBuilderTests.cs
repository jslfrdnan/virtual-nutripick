using NUnit.Framework;
using UnityEditor;
using NutriPick.Content;
using NutriPick.Nutrition;
using NutriPick.Quests;

namespace NutriPick.Tests
{
    public class QuestHintBuilderTests
    {
        [Test]
        public void AnakLevel02FiberHintSuggestsFeasiblePlate()
        {
            var quest = LoadAsset<QuestDefinition>("Assets/ScriptableObjects/Quests/QD_anak.asset");
            var level = LoadAsset<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset");
            var plate = new[]
            {
                Food("ayam_goreng"),
                Food("tempe"),
                Food("sayur_singkong"),
                Food("bayam"),
                Food("ikan_cakalang")
            };

            var evaluation = QuestEvaluator.Evaluate(plate, quest, 2);
            Assert.IsFalse(evaluation.AllPassed, "The near-miss plate should not pass.");

            var failing = evaluation.tasks.Find(task => !task.passed);
            Assert.IsNotNull(failing);
            Assert.AreEqual("SERAT", failing.label);

            var hint = QuestHintBuilder.Build(evaluation, level, plate);
            StringAssert.Contains("SERAT", hint);
            StringAssert.Contains("7,9", hint);
            StringAssert.Contains("Coba:", hint);
            StringAssert.Contains("Bayam", hint);
            StringAssert.Contains("Sayur singkong", hint);
        }

        private static T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.IsNotNull(asset, "Missing asset: " + path);
            return asset;
        }

        private static FoodData Food(string id)
        {
            var asset = AssetDatabase.LoadAssetAtPath<FoodData>(
                "Assets/ScriptableObjects/Food/FD_" + id + ".asset");
            Assert.IsNotNull(asset, "Missing food: " + id);
            return asset;
        }
    }
}
