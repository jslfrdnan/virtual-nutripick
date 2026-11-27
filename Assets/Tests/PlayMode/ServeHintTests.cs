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
    public class ServeHintTests
    {
        [UnityTest]
        public IEnumerator BlockedFiberServeShowsPlateHint()
        {
            GameSession.Instance?.ResetSession();

            SceneManager.LoadScene("Level02_Warteg");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(flow, "LevelFlowController missing.");
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.AreEqual(6, plate.slots.Length, "The serving plate should have six slots.");

            TestQuestUtility.StartLevel(flow, "anak");

            string[] picks = { "nasi_putih", "ayam_goreng", "tempe", "bayam" };
            for (int i = 0; i < picks.Length; i++)
            {
                var item = foods.FirstOrDefault(food => food.data != null && food.data.id == picks[i]);
                Assert.IsNotNull(item, "Missing food: " + picks[i]);
                Assert.IsTrue(plate.slots[i].TryPlace(item), "Could not place " + picks[i]);
            }

            Assert.AreEqual(4, plate.RequiredItems, "The kids quest should require four foods.");

            flow.TryServe();

            Assert.IsFalse(flow.IsReportOpen, "An incomplete fiber task should block the report.");
            var prompt = flow.prompt;
            Assert.IsNotNull(prompt, "Interaction prompt missing.");
            Assert.IsTrue(prompt.root.activeSelf, "The hint prompt should be visible.");
            StringAssert.Contains("SERAT", prompt.text.text);
            StringAssert.Contains("Coba:", prompt.text.text);
            StringAssert.Contains("4,5", prompt.text.text);
        }
    }
}
