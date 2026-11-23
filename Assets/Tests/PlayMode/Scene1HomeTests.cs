using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class Scene1HomeTests
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
        public IEnumerator MainVRHasHomeKitchenAndEightFoodPlates()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(8, foods.Select(food => food.data != null ? food.data.id : null).Distinct().Count(), "Scene 1 should have 8 food types.");
            Assert.AreEqual(13, foods.Length, "Scene 1 should have 13 interactive food items (3 tempe + 2 ayam + 3 brokoli).");
            foreach (var food in foods)
            {
                Assert.IsNotNull(food.data, $"Food {food.name} is missing FoodData.");
                Assert.Greater(food.transform.position.y, 0.75f, $"{food.name} is below the table.");
            }

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(8, stands.Length, "Scene 1 should have eight food plates.");
            Assert.IsTrue(stands.All(stand => stand.name.StartsWith("FoodPlate_")), "Every stand should be a FoodPlate.");
            Assert.AreEqual(8, stands.Sum(stand => stand.foods.Length), "Each food plate should hold one food.");

            Assert.IsNotNull(GameObject.Find("Scene1Home"), "Scene1Home environment root missing.");
            Assert.IsNotNull(GameObject.Find("KitchenRoom"), "KitchenRoom missing.");
            Assert.IsNotNull(GameObject.Find("DiningSet"), "DiningSet missing.");

            Assert.IsNull(GameObject.Find("TableVase"), "TableVase must not be active on the table.");
            Assert.IsNull(GameObject.Find("TablePlant"), "TablePlant must not be active on the table.");
            Assert.IsNull(GameObject.Find("LaukPlatter"), "Old Lauk platter must be removed.");
            Assert.IsNull(GameObject.Find("KarboSayurPlatter"), "Old Karbo platter must be removed.");
            Assert.IsNull(GameObject.Find("Tray_Lauk"), "Old tray must be removed.");
            Assert.IsNull(GameObject.Find("Tray_Karbo"), "Old tray must be removed.");

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.That(plate.transform.position.x, Is.EqualTo(-1.0f).Within(0.1f));
            Assert.That(plate.transform.position.y, Is.EqualTo(0.78f).Within(0.05f));
            Assert.AreEqual(0, plate.Count, "Plate should start empty.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(8, flow.stands.Length, "Flow should know all eight food plates.");
            Assert.IsTrue(flow.IsLevelStarted, "Scene 1 should start immediately.");

            var grids = Object.FindObjectsByType<FoodCardGridPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, grids.Length, "Expected exactly one food card grid.");
            var results = Object.FindObjectsByType<FinalResultPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, results.Length, "Expected exactly one Final Result presenter.");
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, eventSystems.Length, "Expected exactly one EventSystem.");
        }
    }
}
