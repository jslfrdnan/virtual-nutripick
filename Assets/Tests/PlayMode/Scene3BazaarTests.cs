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
            Assert.AreEqual(8, stands.Length, "Level 3 should have eight interactive stalls.");
            Assert.IsTrue(stands.All(stand => stand.foods != null && stand.foods.Length == 1),
                "Every interactive stall should hold exactly one food.");

            var west = stands.Where(stand => stand.transform.position.x < -9f).ToArray();
            var east = stands.Where(stand => stand.transform.position.x > -9f).ToArray();
            Assert.AreEqual(4, west.Length, "Four stalls should sit on the west side of Jl. Flamboyant.");
            Assert.AreEqual(4, east.Length, "Four stalls should sit on the east side of Jl. Flamboyant.");

            CollectionAssert.AreEquivalent(
                new[] { 28, 18, -18, -28 },
                west.Select(stand => Mathf.RoundToInt(stand.transform.position.z)).ToArray(),
                "West stalls should follow the planned north/south rhythm.");
            CollectionAssert.AreEquivalent(
                new[] { 23, 13, -13, -23 },
                east.Select(stand => Mathf.RoundToInt(stand.transform.position.z)).ToArray(),
                "East stalls should follow the planned north/south rhythm.");

            Assert.AreEqual(8, Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None).Length,
                "Every interactive stall should hold one food.");
            Assert.AreEqual(4, Object.FindObjectsByType<DecorStall>(FindObjectsSortMode.None).Length,
                "Four closed decorative stalls should complete the street.");

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
            Assert.AreEqual(8, flow.stands.Length, "Flow should know all eight interactive stalls.");
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
            Assert.AreEqual(4, stalls.Length, "Level 3 should have four decorative stalls.");

            foreach (var stall in stalls)
            {
                Assert.IsNull(stall.GetComponent<FoodStand>(), $"{stall.name} must not be an interactive stand.");
                Assert.AreEqual(0, stall.GetComponentsInChildren<FoodItem>(true).Length,
                    $"{stall.name} must not carry food.");
            }
        }
    }
}
