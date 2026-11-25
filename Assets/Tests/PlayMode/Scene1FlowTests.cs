using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NutriPick.Flow;
using NutriPick.Interaction;

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
        public IEnumerator MainVRHasEightFoodsAndWaitsForQuestChoice()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.AreEqual(8, foods.Select(food => food.data != null ? food.data.id : null).Distinct().Count(), "MainVR should have 8 food types.");
            Assert.AreEqual(8, foods.Length, "MainVR should have one plate per food type.");

            var startScreen = GameObject.Find("StartScreen");
            Assert.IsNull(startScreen, "Start screen should not exist in Scene 1.");

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsFalse(flow.IsLevelStarted, "Scene 1 should wait for a quest choice.");
            Assert.IsTrue(flow.questSelect.IsOpen, "Quest select should be open.");
            Assert.IsFalse(flow.IsReportOpen, "Report should be closed at scene start.");

            flow.questSelect.questButtons[0].onClick.Invoke();
            yield return null;

            Assert.IsTrue(flow.IsLevelStarted, "Choosing a quest should start the level.");
            Assert.IsFalse(flow.questSelect.IsOpen, "Quest select should close after choosing.");
            Assert.IsTrue(GameSession.Instance.HasQuest, "The chosen quest should be stored.");
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
            TestQuestUtility.StartLevel(flow);
            Assert.GreaterOrEqual(foods.Length, 4, "Not enough food for the test.");

            var picks = new[] { "tahu", "telur_rebus", "pisang", "brokoli" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            Assert.AreEqual(4, plate.Count);
            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(plate.IsLocked, "Plate should lock after serving.");
            Assert.IsTrue(flow.IsReportOpen, "Report should open after serving.");
            Assert.IsTrue(flow.finalResult.root.activeSelf, "Result root should be visible.");
        }
    }
}
