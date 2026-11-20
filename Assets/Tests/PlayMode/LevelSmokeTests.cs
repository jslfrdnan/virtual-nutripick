using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.Player;

namespace NutriPick.Tests
{
    public class LevelSmokeTests
    {
        [SetUp]
        public void SetUp()
        {
            if (GameSession.Instance != null)
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator MainVRSpawnsFoodAndWiresHud()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(8, foods.Length, "MainVR should have 8 interactive foods.");
            yield return new WaitForSeconds(0.5f);
            foreach (var food in foods)
                Assert.Greater(food.transform.position.y, 0.75f, $"Food {food.name} fell through the food plate.");

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.AreEqual(4, plate.slots.Length, "Plate should have 4 slots.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(flow.bar, "NutritionBar missing.");
            Assert.IsNotNull(flow.timer, "Timer missing.");
            Assert.IsNotNull(flow.infoCard, "Info card missing.");
            Assert.IsNotNull(flow.report, "Report missing.");
            Assert.IsNotNull(flow.player, "Player missing.");
            Assert.IsNotNull(flow.reticle, "Reticle missing.");
            Assert.AreEqual(1, GameSession.Instance.currentLevelIndex);
        }

        [UnityTest]
        public IEnumerator MainVRCanPlateAndServe()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(foods, "No food.");
            Assert.IsNotNull(plate, "No plate.");
            Assert.IsNotNull(flow, "No flow.");

            flow.StartLevel();
            yield return null;

            Assert.GreaterOrEqual(foods.Length, 3, "Not enough interactive food.");
            for (int i = 0; i < 3; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(foods[i]), "Could not place food " + i);

            yield return null;
            Assert.AreEqual(3, plate.Count, "Plate should contain 3 items.");
            flow.TryServe();
            yield return new WaitForSeconds(1.4f);

            Assert.IsTrue(flow.IsReportOpen, "Report did not open.");
            Assert.IsNotNull(GameSession.Instance.GetResult(1), "Level 1 result was not recorded.");
            var resultRoot = flow.finalResult != null ? flow.finalResult.root : flow.report.root;
            Assert.IsTrue(resultRoot.activeSelf, "Result panel is not active.");
        }

        [UnityTest]
        public IEnumerator Level02SpawnsSevenFoods()
        {
            if (!Application.CanStreamedLevelBeLoaded("Level02_Padang"))
            {
                Assert.Ignore("Level02_Padang is not in the current build settings.");
                yield break;
            }

            SceneManager.LoadScene("Level02_Padang");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(1, stands.Length, "Level 2 should have one stand.");
            Assert.AreEqual(7, stands[0].Spawned.Count, "Level 2 should spawn 7 foods.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(2, flow.level.levelIndex);
        }

        [UnityTest]
        public IEnumerator Level03HasFourStandsAndDrinkRequirement()
        {
            if (!Application.CanStreamedLevelBeLoaded("Level03_Bazaar"))
            {
                Assert.Ignore("Level03_Bazaar is not in the current build settings.");
                yield break;
            }

            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(4, stands.Length, "Level 3 should have four stands.");

            int spawned = 0;
            foreach (var stand in stands)
                spawned += stand.Spawned.Count;
            Assert.AreEqual(8, spawned, "Level 3 should spawn 8 foods across the stands.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(3, flow.level.levelIndex);
            Assert.IsTrue(flow.level.requiresDrink);
            Assert.IsNotNull(flow.plate, "Plate missing.");
            Assert.AreEqual(4, flow.plate.slots.Length);
        }
    }
}
