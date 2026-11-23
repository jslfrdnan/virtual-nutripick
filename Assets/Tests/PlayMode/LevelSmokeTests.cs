using System.Collections;
using System.Linq;
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
                {
                GameSession.Instance.ResetSession();
                GameSession.Instance.MarkIntroSeen();
                GameSession.Instance.sessionActive = true;
            }
        }

        [UnityTest]
        public IEnumerator MainVRSpawnsFoodAndWiresHud()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(8, foods.Select(food => food.data != null ? food.data.id : null).Distinct().Count(), "MainVR should have 8 food types.");
            Assert.AreEqual(13, foods.Length, "MainVR should have 13 interactive food items (3 tempe + 2 ayam + 3 brokoli).");
            yield return new WaitForSeconds(0.5f);
            foreach (var food in foods)
                Assert.Greater(food.transform.position.y, 0.75f, $"Food {food.name} fell through the food plate.");

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.AreEqual(4, plate.slots.Length, "Plate should have 4 slots.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(flow.bar, "NutritionBar missing.");
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

            Assert.GreaterOrEqual(foods.Length, 4, "Not enough interactive food.");
            var picks = new[] { "burger", "rendang", "brokoli", "pisang" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            yield return null;
            Assert.AreEqual(4, plate.Count, "Plate should contain 4 items.");
            flow.TryServe();
            yield return new WaitForSeconds(1.4f);

            Assert.IsTrue(flow.IsReportOpen, "Report did not open.");
            Assert.IsNotNull(GameSession.Instance.GetResult(1), "Level 1 result was not recorded.");
            var resultRoot = flow.finalResult != null ? flow.finalResult.root : flow.report.root;
            Assert.IsTrue(resultRoot.activeSelf, "Result panel is not active.");
        }

        [UnityTest]
        public IEnumerator Level2WartegSpawns21Foods()
        {
            if (!Application.CanStreamedLevelBeLoaded("Level02_Warteg"))
            {
                Assert.Ignore("Level02_Warteg is not in the current build settings.");
                yield break;
            }

            SceneManager.LoadScene("Level02_Warteg");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(1, stands.Length, "Level 2 should have one food display.");
            Assert.AreEqual(7, stands[0].foods.Length, "Level 2 display should hold 7 foods.");

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(21, foods.Length, "Level 2 should have 21 interactive foods (3 portions x 7 dishes).");

            var npcs = Object.FindObjectsByType<NutriPick.NPC.NpcMarker>(FindObjectsSortMode.None);
            Assert.AreEqual(5, npcs.Length, "Level 2 should have five set-dressing NPCs.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(2, flow.level.levelIndex);
            StringAssert.Contains("Warteg", flow.level.displayName);
        }

        [UnityTest]
        public IEnumerator Level03HasThirtyStallsWithoutDrinkRequirement()
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
            Assert.AreEqual(30, stands.Length, "Level 3 should have thirty open stalls.");

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(120, foods.Length, "Level 3 should spawn 120 interactive foods across the thirty stalls.");

            var decor = Object.FindObjectsByType<DecorStall>(FindObjectsSortMode.None);
            Assert.AreEqual(0, decor.Length, "Level 3 should have no closed stalls.");

            var npcs = Object.FindObjectsByType<NutriPick.NPC.NpcMarker>(FindObjectsSortMode.None);
            Assert.AreEqual(9, npcs.Length, "Level 3 should have nine set-dressing NPCs.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(3, flow.level.levelIndex);
            Assert.IsFalse(flow.level.requiresDrink, "Level 3 should not require a drink.");
            Assert.IsNotNull(flow.plate, "Plate missing.");
            Assert.AreEqual(4, flow.plate.slots.Length);
        }
    }
}
