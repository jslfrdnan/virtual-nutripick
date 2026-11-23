using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class Scene1FlowTests
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
        public IEnumerator MainVRHasEightFoodsAndStartsImmediately()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(8, foods.Select(food => food.data != null ? food.data.id : null).Distinct().Count(), "MainVR should have 8 food types.");
            Assert.AreEqual(13, foods.Length, "MainVR should have 13 interactive food items (3 tempe + 2 ayam + 3 brokoli).");

            var startScreen = GameObject.Find("StartScreen");
            Assert.IsNull(startScreen, "Start screen should not exist in Scene 1.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsTrue(flow.IsLevelStarted, "Scene 1 should start immediately.");
            Assert.IsFalse(flow.IsReportOpen, "Report should be closed at scene start.");
        }

        [UnityTest]
        public IEnumerator ServeFourFoodsShowsReport()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(flow.report, "HealthReportPresenter missing.");
            Assert.GreaterOrEqual(foods.Length, 4, "Not enough food for the test.");

            var picks = new[] { "burger", "rendang", "brokoli", "pisang" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            Assert.AreEqual(4, plate.Count);
            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(plate.IsLocked, "Plate should lock after serving.");
            Assert.IsTrue(flow.IsReportOpen, "Report should open after serving.");
            var resultRoot = flow.finalResult != null ? flow.finalResult.root : flow.report.root;
            Assert.IsTrue(resultRoot.activeSelf, "Result root should be visible.");
        }
    }
}
