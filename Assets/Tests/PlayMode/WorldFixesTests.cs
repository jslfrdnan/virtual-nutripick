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
    public class WorldFixesTests
    {
        [SetUp]
        public void SetUp()
        {
            if (GameSession.Instance != null)
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator ServingPlateSitsOnTheCashierTable()
        {
            SceneManager.LoadScene("Level02_Warteg");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            var plateVisual = plate.GetComponentsInChildren<Renderer>(true)
                .FirstOrDefault(renderer => renderer.name == "PlateVisual");
            var cashierTop = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .FirstOrDefault(renderer => renderer.name == "CashierTop");
            Assert.IsNotNull(plateVisual, "PlateVisual missing.");
            Assert.IsNotNull(cashierTop, "CashierTop missing.");

            Assert.Less(plateVisual.bounds.size.y, 0.05f, "The plate must lie flat, not stand on its rim.");
            Assert.GreaterOrEqual(plateVisual.bounds.min.y, cashierTop.bounds.max.y - 0.006f,
                "The plate must not sink into the cashier table.");
            Assert.LessOrEqual(plateVisual.bounds.min.y, cashierTop.bounds.max.y + 0.03f,
                "The plate must sit on, not float above, the cashier table.");
        }

        [UnityTest]
        public IEnumerator ServingPlateSitsOnTheStageCounter()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.IsNotNull(GameObject.Find("ServingTable"), "Serving table missing.");
            var plateVisual = plate.GetComponentsInChildren<Renderer>(true)
                .FirstOrDefault(renderer => renderer.name == "PlateVisual");
            Assert.IsNotNull(plateVisual, "PlateVisual missing.");

            var counter = Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None)
                .Where(collider => collider.gameObject.name == "Collider_Counter")
                .OrderBy(collider => Vector3.Distance(collider.bounds.center, plate.transform.position))
                .FirstOrDefault();
            Assert.IsNotNull(counter, "Stage counter collider missing.");

            Assert.Less(plateVisual.bounds.size.y, 0.05f, "The plate must lie flat, not stand on its rim.");
            Assert.GreaterOrEqual(plateVisual.bounds.min.y, counter.bounds.max.y - 0.006f,
                "The plate must not sink into the stage counter.");
            Assert.LessOrEqual(plateVisual.bounds.min.y, counter.bounds.max.y + 0.04f,
                "The plate must sit on the stage counter.");
        }

        [UnityTest]
        public IEnumerator ServingPlateSitsOnTheDiningTable()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            var plateVisual = plate.GetComponentsInChildren<Renderer>(true)
                .FirstOrDefault(renderer => renderer.name == "PlateVisual");
            var tableTop = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                .FirstOrDefault(renderer => renderer.name == "DiningTableTop");
            Assert.IsNotNull(plateVisual, "PlateVisual missing.");
            Assert.IsNotNull(tableTop, "DiningTableTop missing.");

            Assert.Less(plateVisual.bounds.size.y, 0.05f, "The plate must lie flat, not stand on its rim.");
            Assert.GreaterOrEqual(plateVisual.bounds.min.y, tableTop.bounds.max.y - 0.006f,
                "The plate must not sink into the dining table.");
            Assert.LessOrEqual(plateVisual.bounds.min.y, tableTop.bounds.max.y + 0.03f,
                "The plate must sit on the dining table.");
        }

        [UnityTest]
        public IEnumerator HeldFoodFollowsPlayerInLevel2()
        {
            yield return CheckHeldFoodFollows("Level02_Warteg");
        }

        [UnityTest]
        public IEnumerator HeldFoodFollowsPlayerInLevel3()
        {
            yield return CheckHeldFoodFollows("Level03_Bazaar");
        }

        private IEnumerator CheckHeldFoodFollows(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return null;

            var carrier = Object.FindFirstObjectByType<HeldFoodCarrier>();
            var player = Object.FindFirstObjectByType<FirstPersonController>();
            var food = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None)
                .FirstOrDefault(item => item.data != null);
            Assert.IsNotNull(carrier, "HeldFoodCarrier missing.");
            Assert.IsNotNull(player, "FirstPersonController missing.");
            Assert.IsNotNull(food, "No food item found.");

            carrier.Hold(food);
            yield return null;

            Assert.AreEqual(carrier.holdAnchor, food.transform.parent,
                "Held food must be parented to the hold anchor.");
            Assert.IsTrue(food.body == null || food.body.isKinematic,
                "Held food must be kinematic.");

            Vector3 playerBefore = player.transform.position;
            Vector3 foodBefore = food.transform.position;
            player.transform.position += new Vector3(1.5f, 0f, 0f);
            yield return null;

            Vector3 playerDelta = player.transform.position - playerBefore;
            Vector3 foodDelta = food.transform.position - foodBefore;
            Assert.That(foodDelta.x, Is.EqualTo(playerDelta.x).Within(0.02f),
                "Held food did not follow the player in " + sceneName + ".");
            Assert.That(foodDelta.z, Is.EqualTo(playerDelta.z).Within(0.02f),
                "Held food did not follow the player in " + sceneName + ".");

            carrier.Release();
            food.ReturnHome();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Scene2HasTheShophouseWorld()
        {
            SceneManager.LoadScene("Level02_Warteg");
            yield return null;
            yield return null;

            var names = new[]
            {
                "Ruko_West_1", "Ruko_West_2", "Ruko_East_1", "Ruko_East_2",
                "Building_North_1", "Building_North_2"
            };

            foreach (var name in names)
                Assert.IsNotNull(GameObject.Find(name), "Missing " + name);

            Assert.IsNotNull(GameObject.Find("StreetFence_Rail"), "Missing street fence.");
        }

        [UnityTest]
        public IEnumerator Level3HasNoDrinks()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(15, foods.Length, "Level 3 should have 15 foods.");
            Assert.IsFalse(foods.Any(food => food.data != null && food.data.isDrink),
                "Level 3 must not contain drink items.");

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            var healthy = stands.First(stand => stand.displayName == "Healthy Food");
            var junk = stands.First(stand => stand.displayName == "Junk Food");
            Assert.AreEqual(4, healthy.foods.Length, "Healthy Food should hold four foods.");
            Assert.AreEqual(2, junk.foods.Length, "Junk Food should hold two foods.");
        }
    }
}
