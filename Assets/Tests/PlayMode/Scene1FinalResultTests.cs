using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
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
                GameSession.Instance.ResetSession();
        }

        [UnityTest]
        public IEnumerator FinalResultShowsTwoPanelsAndActions()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var start = Object.FindFirstObjectByType<StartScreenPresenter>();
            Assert.IsNotNull(start, "StartScreenPresenter missing.");
            start.startButton.onClick.Invoke();
            yield return null;

            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var finalResult = Object.FindFirstObjectByType<FinalResultPresenter>(FindObjectsInactive.Include);
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(finalResult, "FinalResultPresenter missing.");
            Assert.GreaterOrEqual(foods.Length, 3, "Not enough food.");

            for (int i = 0; i < 3; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(foods[i]), "Could not place food " + i);

            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(flow.IsReportOpen, "Report state should be open.");
            Assert.IsTrue(finalResult.root.activeSelf, "Final Result root should be visible.");
            Assert.AreEqual("FINAL RESULT", finalResult.titleText.text);
            Assert.AreEqual("LEBIH SEHAT", finalResult.balancedBadgeText.text);
            Assert.AreEqual("PERLU DIUBAH", finalResult.lessBalancedBadgeText.text);
            Assert.AreEqual(3, finalResult.balancedPointTexts.Length);
            Assert.AreEqual(3, finalResult.lessBalancedPointTexts.Length);

            foreach (var point in finalResult.balancedPointTexts)
                Assert.IsFalse(string.IsNullOrWhiteSpace(point.text));
            foreach (var point in finalResult.lessBalancedPointTexts)
                Assert.IsFalse(string.IsNullOrWhiteSpace(point.text));

            Assert.IsTrue(finalResult.retryButton.gameObject.activeSelf, "Retry should be visible.");
            Assert.IsTrue(finalResult.nextButton.gameObject.activeSelf, "Next should be visible.");
        }
    }
}
