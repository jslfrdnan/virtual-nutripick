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
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator Level03HasDoubleSidedBazaarAndServingStage()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.AreEqual(2, stands.Length, "Level 3 should have two open stalls.");
            Assert.IsTrue(stands.All(stand => stand.foods != null && stand.foods.Length > 0),
                "Every open stall should hold at least one food.");
            Assert.IsTrue(stands.Any(stand => stand.displayName == "Healthy Food"),
                "Level 3 should have a Healthy Food stall.");
            Assert.IsTrue(stands.Any(stand => stand.displayName == "Junk Food"),
                "Level 3 should have a Junk Food stall.");

            var healthy = stands.First(stand => stand.displayName == "Healthy Food");
            var junk = stands.First(stand => stand.displayName == "Junk Food");
            Assert.AreEqual(5, healthy.foods.Length, "Healthy Food should hold five foods.");
            Assert.AreEqual(3, junk.foods.Length, "Junk Food should hold three foods.");
            Assert.Greater(healthy.transform.position.z, 0f, "Healthy Food should sit on the north side.");
            Assert.Less(junk.transform.position.z, 0f, "Junk Food should sit on the south side.");

            var foodItems = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(17, foodItems.Length,
                "Four bananas, three tempe slices, three broccoli florets, three ayam goreng pieces, and the other four foods should be available across the two stalls.");
            Assert.AreEqual(8, foodItems.Select(food => food.data).Distinct().Count(),
                "All eight unique foods should still be available across the two stalls.");
            Assert.AreEqual(10, Object.FindObjectsByType<DecorStall>(FindObjectsSortMode.None).Length,
                "Ten closed stalls should complete the street.");

            var plate = Object.FindFirstObjectByType<PlateController>();
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.AreEqual(4, plate.slots.Length, "Serving plate should have four slots.");
            Assert.That(plate.transform.position.x, Is.EqualTo(-8.35f).Within(0.2f));
            Assert.That(plate.transform.position.y, Is.EqualTo(0.78f).Within(0.05f));
            Assert.That(plate.transform.position.z, Is.EqualTo(2.35f).Within(0.2f));

            var station = Object.FindFirstObjectByType<ServeStation>();
            Assert.IsNotNull(station, "Serve station missing.");
            Assert.That(station.transform.position.z, Is.EqualTo(2.35f).Within(0.2f));

            Assert.IsNotNull(GameObject.Find("Scene3Bazaar"), "Scene3Bazaar root missing.");
            Assert.IsNotNull(GameObject.Find("SchoolCampus"), "School backdrop missing.");
            Assert.IsNotNull(GameObject.Find("Cafe_Kopi"), "North cafe missing.");
            Assert.IsNotNull(GameObject.Find("Cafe_Bakar"), "South cafe missing.");
            Assert.IsNotNull(GameObject.Find("PanggungBazaar"), "Bazaar stage missing.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.AreEqual(3, flow.level.levelIndex);
            Assert.IsTrue(flow.IsLevelStarted, "Level 3 should start immediately.");
            Assert.IsTrue(flow.level.requiresDrink, "Level 3 should require a drink.");
            Assert.AreEqual(2, flow.stands.Length, "Flow should know both open stalls.");
            Assert.Less(flow.serveStation.transform.position.x, 2f, "Serving must happen outside the school campus.");

            var grids = Object.FindObjectsByType<FoodCardGridPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, grids.Length, "Expected exactly one food card grid.");
            var results = Object.FindObjectsByType<FinalResultPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, results.Length, "Expected exactly one Final Result presenter.");
            var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, eventSystems.Length, "Expected exactly one EventSystem.");
        }

        [UnityTest]
        public IEnumerator DecorativeStallsNeverCarryFood()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var stalls = Object.FindObjectsByType<DecorStall>(FindObjectsSortMode.None);
            Assert.AreEqual(10, stalls.Length, "Level 3 should have ten closed stalls.");

            foreach (var stall in stalls)
            {
                Assert.IsNull(stall.GetComponent<FoodStand>(), $"{stall.name} must not be an interactive stand.");
                Assert.AreEqual(0, stall.GetComponentsInChildren<FoodItem>(true).Length,
                    $"{stall.name} must not carry food.");
            }
        }

        [UnityTest]
        public IEnumerator StandPopularityTracksPlateChoices()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var tracker = Object.FindFirstObjectByType<StandPopularityTracker>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(tracker, "StandPopularityTracker missing.");
            Assert.IsNotNull(plate, "Serving plate missing.");
            Assert.AreEqual(0, tracker.HealthyCount, "Healthy count should start at zero.");
            Assert.AreEqual(0, tracker.JunkCount, "Junk count should start at zero.");

            var healthyFood = foods.First(food => food.data != null && food.data.id == "brokoli");
            var junkFood = foods.First(food => food.data != null && food.data.id == "ayam_goreng");

            Assert.IsTrue(plate.slots[0].TryPlace(healthyFood), "Could not plate brokoli.");
            yield return null;
            Assert.AreEqual(1, tracker.HealthyCount, "Plating brokoli should count for Healthy Food.");
            Assert.AreEqual(0, tracker.JunkCount);

            Assert.IsTrue(plate.slots[1].TryPlace(junkFood), "Could not plate ayam goreng.");
            yield return null;
            Assert.AreEqual(1, tracker.HealthyCount);
            Assert.AreEqual(1, tracker.JunkCount, "Plating ayam goreng should count for Junk Food.");

            plate.slots[0].Remove(healthyFood);
            yield return null;
            Assert.AreEqual(0, tracker.HealthyCount, "Removing brokoli should lower the Healthy count.");
            Assert.AreEqual(1, tracker.JunkCount);
        }

        [UnityTest]
        public IEnumerator VendorsAndScoreboardExist()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var vendors = Object.FindObjectsByType<StandVendor>(FindObjectsSortMode.None);
            Assert.AreEqual(2, vendors.Length, "Level 3 should have one vendor per open stand.");
            Assert.IsNotNull(Object.FindFirstObjectByType<StandScoreboard>(), "StandScoreboard missing.");

            var stands = Object.FindObjectsByType<FoodStand>(FindObjectsSortMode.None);
            Assert.IsTrue(stands.Any(stand => stand.kind == FoodStandKind.Healthy));
            Assert.IsTrue(stands.Any(stand => stand.kind == FoodStandKind.Junk));
        }

        [UnityTest]
        public IEnumerator VendorBubbleAppearsNearStand()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var player = Object.FindFirstObjectByType<FirstPersonController>();
            var vendor = Object.FindObjectsByType<StandVendor>(FindObjectsSortMode.None)
                .First(item => item.stand != null && item.stand.kind == FoodStandKind.Healthy);
            Assert.IsNotNull(player, "Player missing.");
            Assert.IsNotNull(vendor, "Healthy vendor missing.");
            Assert.IsNotNull(vendor.bubbleRoot, "Vendor speech bubble missing.");

            player.transform.position = new Vector3(-9f, 0.02f, 41.5f);
            yield return null;
            yield return null;
            Assert.IsFalse(vendor.bubbleRoot.activeSelf, "Bubble should be hidden when the player is far away.");

            player.transform.position = vendor.transform.position + new Vector3(2.5f, 0f, 0f);
            yield return null;
            yield return null;
            Assert.IsTrue(vendor.bubbleRoot.activeSelf, "Bubble should appear when the player is near the stand.");
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

            var picks = new[] { "brokoli", "pisang", "ayam_goreng", "teh_manis" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();

            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id + ".");

            yield return null;
            Assert.IsTrue(plate.CanServe(out string reason), reason);
            Assert.AreEqual("A: Sajikan", flow.GetServeReason(),
                "A mixed healthy + junk plate should be serveable.");
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

            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");
            var ayam = foods.First(food => food.data != null && food.data.id == "ayam_goreng");

            carrier.Hold(brokoli);
            yield return null;
            Assert.AreEqual("Sehat 1 · Kurang sehat 0", flow.mealStatus.countsLabel.text,
                "Holding a healthy food should preview it immediately.");

            carrier.Release();
            yield return null;
            Assert.AreEqual("Sehat 0 · Kurang sehat 0", flow.mealStatus.countsLabel.text,
                "Releasing the food should clear the held preview.");

            Assert.IsTrue(plate.slots[0].TryPlace(brokoli));
            carrier.Hold(ayam);
            yield return null;
            Assert.AreEqual("Sehat 1 · Kurang sehat 1", flow.mealStatus.countsLabel.text,
                "The preview should combine plated and held foods.");

            carrier.Release();
            yield return null;
            Assert.AreEqual("Sehat 1 · Kurang sehat 0", flow.mealStatus.countsLabel.text,
                "Releasing the junk food should leave only the plated healthy food.");
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

            var picks = new[] { "air_putih", "ayam_goreng", "telur_rebus", "tempe" }
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
