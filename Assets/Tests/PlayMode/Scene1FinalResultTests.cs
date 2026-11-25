using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.UI;

namespace NutriPick.Tests
{
    public class Scene1FinalResultTests
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
        public IEnumerator FinalResultShowsTasksNutritionAndActions()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var finalResult = Object.FindFirstObjectByType<FinalResultPresenter>(FindObjectsInactive.Include);
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(finalResult, "FinalResultPresenter missing.");
            TestQuestUtility.StartLevel(flow);
            Assert.GreaterOrEqual(foods.Length, 4, "Not enough food.");

            var picks = new[] { "tahu", "telur_rebus", "pisang", "brokoli" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(flow.IsReportOpen, "Report state should be open.");
            Assert.IsTrue(finalResult.root.activeSelf, "Final Result root should be visible.");
            Assert.AreEqual("HASIL LEVEL", finalResult.titleText.text);
            StringAssert.Contains("Anak-anak", finalResult.subtitleText.text);
            StringAssert.Contains("[x]", finalResult.taskText.text);
            StringAssert.Contains("PROTEIN", finalResult.taskText.text);
            StringAssert.Contains("GIZI PIRING", finalResult.nutritionText.text);
            StringAssert.Contains("PROTEIN:", finalResult.nutritionText.text);

            Assert.IsTrue(finalResult.retryButton.gameObject.activeSelf, "Retry should be visible.");
            Assert.IsTrue(finalResult.nextButton.gameObject.activeSelf, "Next should be visible.");
        }

        [UnityTest]
        public IEnumerator FinalResultFocusesRetryAndSupportsControllerNavigation()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var finalResult = Object.FindFirstObjectByType<FinalResultPresenter>(FindObjectsInactive.Include);
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(finalResult, "FinalResultPresenter missing.");
            TestQuestUtility.StartLevel(flow);

            var picks = new[] { "tahu", "telur_rebus", "pisang", "brokoli" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsNotNull(EventSystem.current, "EventSystem missing.");
            Assert.AreEqual(finalResult.retryButton.gameObject, EventSystem.current.currentSelectedGameObject,
                "Retry should receive the initial controller focus.");

            var retryNavigation = finalResult.retryButton.navigation;
            Assert.AreEqual(Navigation.Mode.Explicit, retryNavigation.mode, "Retry navigation should be explicit.");
            Assert.AreEqual(finalResult.nextButton, retryNavigation.selectOnRight, "Retry right should reach Next.");
            Assert.AreEqual(finalResult.retryButton, finalResult.nextButton.navigation.selectOnLeft,
                "Next left should return to Retry.");
            Assert.AreEqual(finalResult.nextButton, finalResult.retryButton.FindSelectableOnRight(),
                "Controller right should select Next.");

            var colors = finalResult.retryButton.colors;
            Assert.Greater(colors.selectedColor.grayscale - colors.normalColor.grayscale, 0.15f,
                "Selected color must be visibly brighter than normal.");
            Assert.IsNotNull(finalResult.retryButton.GetComponent<SelectableFocusHighlight>(),
                "Retry should carry the focus highlight component.");
            Assert.IsNotNull(finalResult.nextButton.GetComponent<SelectableFocusHighlight>(),
                "Next should carry the focus highlight component.");
            Assert.IsNotNull(finalResult.inputHint, "Controller hint should be present.");
            StringAssert.Contains("D-pad", finalResult.inputHint.text);
            StringAssert.Contains("A / Enter", finalResult.inputHint.text);
        }
    }
}
