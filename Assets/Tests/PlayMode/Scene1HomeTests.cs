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
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator MainVRHasHomeKitchenAndTwoPlatters()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(8, foods.Length, "Scene 1 should have 8 interactive foods.");
            foreach (var food in foods)
                Assert.IsNotNull(food.data, $"Food {food.name} is missing FoodData.");

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(2, stands.Length, "Scene 1 should have two serving platters.");
            var lauk = stands.FirstOrDefault(stand => stand.name == "LaukPlatter");
            var karbo = stands.FirstOrDefault(stand => stand.name == "KarboSayurPlatter");
            Assert.IsNotNull(lauk, "LaukPlatter missing.");
            Assert.IsNotNull(karbo, "KarboSayurPlatter missing.");
            Assert.AreEqual(5, lauk.foods.Length, "LaukPlatter should hold 5 foods.");
            Assert.AreEqual(3, karbo.foods.Length, "KarboSayurPlatter should hold 3 foods.");

            Assert.IsNotNull(GameObject.Find("Scene1Home"), "Scene1Home environment root missing.");
            Assert.IsNotNull(GameObject.Find("KitchenRoom"), "KitchenRoom missing.");
            Assert.IsNotNull(GameObject.Find("DiningSet"), "DiningSet missing.");

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.That(plate.transform.position.x, Is.EqualTo(-1.0f).Within(0.1f));
            Assert.That(plate.transform.position.y, Is.EqualTo(0.78f).Within(0.05f));
            Assert.AreEqual(0, plate.Count, "Plate should start empty.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(2, flow.stands.Length, "Flow should know both platters.");
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
