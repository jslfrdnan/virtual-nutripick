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
using NutriPick.Player;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class Scene3BazaarTests
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
        public IEnumerator Level03HasDoubleSidedBazaarAndServingStage()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(30, stands.Length, "Level 3 should have thirty open food stalls.");
            Assert.IsTrue(stands.All(stand => stand.foods != null && stand.foods.Length == 2),
                "Every open stall should hold exactly two food types.");

            var foodItems = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(120, foodItems.Length, "Level 3 should hold 120 interactive foods.");
            Assert.AreEqual(18, foodItems.Select(food => food.data).Distinct().Count(),
                "All eighteen unique foods should be available across the stalls.");
            Assert.AreEqual(0, Object.FindObjectsByType<DecorStall>(FindObjectsSortMode.None).Length,
                "Level 3 should not have closed decorative stalls anymore.");

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.AreEqual(4, plate.slots.Length, "Serving plate should have four slots.");
            Assert.That(plate.transform.position.x, Is.EqualTo(-8.35f).Within(0.2f));
            Assert.That(plate.transform.position.y, Is.EqualTo(0.652f).Within(0.02f));
            Assert.That(plate.transform.position.z, Is.EqualTo(3.65f).Within(0.2f));

            var station = Object.FindFirstObjectByType<ServeStation>();
            Assert.IsNotNull(station, "Serve station missing.");
            Assert.That(station.transform.position.z, Is.EqualTo(3.65f).Within(0.2f));

            Assert.IsNotNull(GameObject.Find("Scene3Bazaar"), "Scene3Bazaar root missing.");
            Assert.IsNotNull(GameObject.Find("SchoolCampus"), "School backdrop missing.");
            Assert.IsNotNull(GameObject.Find("Cafe_Kopi"), "North cafe missing.");
            Assert.IsNotNull(GameObject.Find("Cafe_Bakar"), "South cafe missing.");
            Assert.IsNotNull(GameObject.Find("PanggungBazaar"), "Bazaar stage missing.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            TestQuestUtility.StartLevel(flow);
            Assert.AreEqual(3, flow.level.levelIndex);
            Assert.IsTrue(flow.IsLevelStarted, "Level 3 should start after a quest is chosen.");
            Assert.IsFalse(flow.level.requiresDrink, "Level 3 should not require a drink.");
            Assert.AreEqual(30, flow.stands.Length, "Flow should know all thirty open stalls.");
            Assert.Less(flow.serveStation.transform.position.x, 2f, "Serving must happen outside the school campus.");

            var player = Object.FindFirstObjectByType<FirstPersonController>();
            Assert.IsNotNull(player, "Player missing.");
            Assert.That(player.transform.position.x, Is.EqualTo(-8.35f).Within(0.5f));
            Assert.That(player.transform.position.z, Is.EqualTo(6.2f).Within(0.6f),
                "Scene 3 should spawn the player in front of the serving plate.");

            Assert.IsNotNull(GameObject.Find("Road_Flamboyant_Cross"), "North cross road missing.");
            Assert.IsNotNull(GameObject.Find("Intersection_Plaza"), "Flamboyant intersection missing.");
            Assert.IsNotNull(GameObject.Find("CoffeeShop_01"), "CoffeeShop_01 missing.");
            Assert.IsNotNull(GameObject.Find("CoffeeShop_04"), "CoffeeShop_04 missing.");
            Assert.IsNotNull(GameObject.Find("SMK_Building"), "SMK building missing.");

            var grids = Object.FindObjectsByType<FoodCardGridPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, grids.Length, "Expected exactly one food card grid.");
            var results = Object.FindObjectsByType<FinalResultPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, results.Length, "Expected exactly one Final Result presenter.");
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, eventSystems.Length, "Expected exactly one EventSystem.");
        }

        [UnityTest]
        public IEnumerator MixedPlateCanServe()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            TestQuestUtility.StartLevel(flow);

            var picks = new[] { "tahu", "brokoli", "jeruk", "pisang" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();

            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id + ".");

            yield return null;
            Assert.IsTrue(plate.CanServe(out string reason), reason);
            Assert.AreEqual("A: Sajikan", flow.GetServeReason(),
                "A plate that meets the dinner tasks should be serveable.");
        }

        [UnityTest]
        public IEnumerator HeldFoodUpdatesPreview()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var carrier = Object.FindFirstObjectByType<HeldFoodCarrier>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(carrier, "HeldFoodCarrier missing.");
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.IsNotNull(flow.mealStatus, "Meal status HUD missing.");
            TestQuestUtility.StartLevel(flow);

            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");
            var ayam = foods.First(food => food.data != null && food.data.id == "ayam_goreng");

            carrier.Hold(brokoli);
            yield return null;
            StringAssert.Contains("SAYUR [x]", flow.mealStatus.countsLabel.text,
                "Holding a vegetable should check the vegetable requirement immediately.");
            StringAssert.Contains("PROTEIN [ ]", flow.mealStatus.countsLabel.text);

            carrier.Release();
            yield return null;
            StringAssert.Contains("SAYUR [ ]", flow.mealStatus.countsLabel.text,
                "Releasing the food should clear the held preview.");

            Assert.IsTrue(plate.slots[0].TryPlace(brokoli));
            carrier.Hold(ayam);
            yield return null;
            StringAssert.Contains("PROTEIN [x]", flow.mealStatus.countsLabel.text,
                "The preview should combine plated and held foods.");

            carrier.Release();
            yield return null;
            StringAssert.Contains("PROTEIN [ ]", flow.mealStatus.countsLabel.text,
                "Releasing the junk food should leave only the plated vegetable.");
        }

        [UnityTest]
        public IEnumerator FoodPortionsAreNotStackedOnTheSameSpot()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;
            yield return new WaitForSeconds(0.1f);

            var activeScene = SceneManager.GetActiveScene();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None)
                .Where(food => food.gameObject.scene == activeScene)
                .ToArray();
            Assert.AreEqual(120, foods.Length, "Level 3 should keep every interactive food portion.");

            for (int i = 0; i < foods.Length; i++)
            {
                for (int j = i + 1; j < foods.Length; j++)
                {
                    float distance = Vector3.Distance(foods[i].transform.position, foods[j].transform.position);
                    Assert.Greater(distance, 0.02f,
                        $"{foods[i].name} and {foods[j].name} are stacked on the same spot ({distance:F3} m).");
                }
            }
        }

        [UnityTest]
        public IEnumerator PlayerCameraStaysAtEyeHeight()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var player = Object.FindFirstObjectByType<FirstPersonController>();
            var camera = Camera.main;
            Assert.IsNotNull(player, "First-person controller missing.");
            Assert.IsNotNull(camera, "Main camera missing.");
            Assert.Less(camera.transform.localPosition.sqrMagnitude, 0.0001f,
                "Main camera must sit at the player's eye pivot, not offset by a screenshot tool.");
            Assert.Less(Quaternion.Angle(camera.transform.localRotation, Quaternion.identity), 0.1f,
                "Main camera must not be rotated relative to the player pivot.");
            Assert.That(camera.transform.position.y, Is.EqualTo(1.6f).Within(0.3f),
                "Main camera should be around eye height.");
        }

        [UnityTest]
        public IEnumerator Level03CanServeAtPanggungBazaar()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.IsNotNull(flow, "LevelFlowController missing.");

            flow.StartLevel();
            yield return null;

            var picks = new[] { "tahu", "brokoli", "jeruk", "pisang" }
                .Select(id => foods.First(food => food.data.id == id))
                .ToArray();

            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), $"Could not place {picks[i].data.id}.");

            yield return null;
            Assert.AreEqual(4, plate.Count, "Plate should contain four items.");
            flow.TryServe();
            yield return new WaitForSeconds(1.4f);

            Assert.IsTrue(flow.IsReportOpen, "Report did not open after serving at the stage.");
            Assert.IsNotNull(GameSession.Instance.GetResult(3), "Level 3 result was not recorded.");
        }
    }
}
