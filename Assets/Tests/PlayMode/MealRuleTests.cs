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
    public class MealRuleTests
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
        public IEnumerator FourFoodsAreRequiredBeforeServe()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Plate missing.");
            TestQuestUtility.StartLevel(flow);

            var tahu = foods.First(food => food.data != null && food.data.id == "tahu");
            var pisang = foods.First(food => food.data != null && food.data.id == "pisang");
            var telur = foods.First(food => food.data != null && food.data.id == "telur_rebus");
            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");

            Assert.IsTrue(plate.slots[0].TryPlace(tahu));
            Assert.IsTrue(plate.slots[1].TryPlace(pisang));
            Assert.IsTrue(plate.slots[2].TryPlace(telur));
            yield return null;

            Assert.IsFalse(plate.CanServe(out string threeItemReason), "Three items must not be serveable.");
            StringAssert.Contains("Tambahkan 1 makanan", threeItemReason);

            Assert.IsTrue(plate.slots[3].TryPlace(brokoli));
            yield return null;
            Assert.IsTrue(plate.CanServe(out string fourItemReason), fourItemReason);
        }

        [UnityTest]
        public IEnumerator MissingFruitBlocksServe()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Plate missing.");
            TestQuestUtility.StartLevel(flow);

            var tahu = foods.First(food => food.data != null && food.data.id == "tahu");
            var telur = foods.First(food => food.data != null && food.data.id == "telur_rebus");
            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");
            var wortel = foods.First(food => food.data != null && food.data.id == "wortel");

            Assert.IsTrue(plate.slots[0].TryPlace(tahu));
            Assert.IsTrue(plate.slots[1].TryPlace(telur));
            Assert.IsTrue(plate.slots[2].TryPlace(brokoli));
            Assert.IsTrue(plate.slots[3].TryPlace(wortel));
            yield return null;

            Assert.IsFalse(plate.CanServe(out string reason), "A plate without fruit must not be serveable.");
            StringAssert.Contains("BUAH", reason);
        }

        [UnityTest]
        public IEnumerator MissingNutrientBlocksServe()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Plate missing.");
            TestQuestUtility.StartLevel(flow, "dewasa");

            var nasi = foods.First(food => food.data != null && food.data.id == "nasi_putih");
            var telur = foods.First(food => food.data != null && food.data.id == "telur_rebus");
            var pisang = foods.First(food => food.data != null && food.data.id == "pisang");
            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");

            Assert.IsTrue(plate.slots[0].TryPlace(nasi));
            Assert.IsTrue(plate.slots[1].TryPlace(telur));
            Assert.IsTrue(plate.slots[2].TryPlace(pisang));
            Assert.IsTrue(plate.slots[3].TryPlace(brokoli));
            yield return null;

            Assert.IsFalse(plate.CanServe(out string reason), "A low-fiber plate must not be serveable.");
            StringAssert.Contains("PROTEIN", reason);
        }

        [UnityTest]
        public IEnumerator HealthyFourItemPlateShowsFinalResult()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Plate missing.");
            Assert.IsNotNull(flow, "Flow missing.");
            TestQuestUtility.StartLevel(flow);

            var tahu = foods.First(food => food.data != null && food.data.id == "tahu");
            var pisang = foods.First(food => food.data != null && food.data.id == "pisang");
            var telur = foods.First(food => food.data != null && food.data.id == "telur_rebus");
            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");

            Assert.IsTrue(plate.slots[0].TryPlace(tahu));
            Assert.IsTrue(plate.slots[1].TryPlace(pisang));
            Assert.IsTrue(plate.slots[2].TryPlace(telur));
            Assert.IsTrue(plate.slots[3].TryPlace(brokoli));

            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(flow.IsReportOpen, "Report should open.");
            Assert.IsTrue(flow.finalResult.root.activeSelf, "Final Result should be visible.");
        }
    }
}
