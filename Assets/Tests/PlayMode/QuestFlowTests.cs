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
    public class QuestFlowTests
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
        public IEnumerator QuestSelectShowsThreeGroups()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsTrue(flow.questSelect.IsOpen, "Quest select should open.");

            int activeButtons = flow.questSelect.questButtons.Count(button => button != null && button.gameObject.activeSelf);
            Assert.AreEqual(3, activeButtons, "Quest select should offer three groups.");
            Assert.IsNotNull(flow.questSelect.questTitles[0].text);
            StringAssert.Contains("ANAK", flow.questSelect.questTitles[0].text);
        }

        [UnityTest]
        public IEnumerator InfoCardShowsMicronutrients()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var infoCard = Object.FindFirstObjectByType<FoodInfoCardPresenter>(FindObjectsInactive.Include);
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(infoCard, "Info card missing.");
            TestQuestUtility.StartLevel(flow);

            var tahu = foods.First(food => food.data != null && food.data.id == "tahu");
            infoCard.ShowFull(tahu.data);
            yield return null;

            StringAssert.Contains("ENERGI", infoCard.fullStats.text);
            StringAssert.Contains("VIT A", infoCard.fullStats.text);
            StringAssert.Contains("VIT D", infoCard.fullStats.text);
            StringAssert.Contains("KALSIUM", infoCard.fullStats.text);
            StringAssert.Contains("BESI", infoCard.fullStats.text);
        }

        [UnityTest]
        public IEnumerator CampaignSummaryAppearsAfterFinalLevel()
        {
            SceneManager.LoadScene("Level03_Bazaar");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(plate, "Plate missing.");
            TestQuestUtility.StartLevel(flow);

            var picks = new[] { "tahu", "brokoli", "jeruk", "pisang" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            flow.TryServe();
            yield return new WaitForSeconds(1.4f);

            Assert.IsTrue(flow.finalResult.root.activeSelf, "Final result should open.");
            flow.finalResult.nextButton.onClick.Invoke();
            yield return null;

            Assert.IsTrue(flow.campaignSummary.root.activeSelf, "Campaign summary should open.");
            StringAssert.Contains("TOTAL HARIAN", flow.campaignSummary.totalsText.text);
            StringAssert.Contains("PROTEIN", flow.campaignSummary.totalsText.text);
            StringAssert.Contains("HASIL PER LEVEL", flow.campaignSummary.levelsText.text);
        }
    }
}
