using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public IEnumerator FourCategoriesAreRequiredBeforeServe()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Plate missing.");

            var burger = foods.First(food => food.data != null && food.data.id == "burger");
            var rendang = foods.First(food => food.data != null && food.data.id == "rendang");
            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");
            var pisang = foods.First(food => food.data != null && food.data.id == "pisang");

            Assert.IsTrue(plate.slots[0].TryPlace(burger));
            Assert.IsTrue(plate.slots[1].TryPlace(rendang));
            Assert.IsTrue(plate.slots[2].TryPlace(brokoli));
            yield return null;

            Assert.IsFalse(plate.CanServe(out string threeItemReason), "Three items must not be serveable.");
            StringAssert.Contains("Tambahkan 1 makanan", threeItemReason);

            Assert.IsTrue(plate.slots[3].TryPlace(pisang));
            yield return null;
            Assert.IsTrue(plate.CanServe(out string fourItemReason), fourItemReason);
        }

        [UnityTest]
        public IEnumerator PlateWithoutVegetableCannotServe()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Plate missing.");

            var burger = foods.First(food => food.data != null && food.data.id == "burger");
            var rendang = foods.First(food => food.data != null && food.data.id == "rendang");
            var telur = foods.First(food => food.data != null && food.data.id == "telur_rebus");
            var pisang = foods.First(food => food.data != null && food.data.id == "pisang");

            Assert.IsTrue(plate.slots[0].TryPlace(burger));
            Assert.IsTrue(plate.slots[1].TryPlace(rendang));
            Assert.IsTrue(plate.slots[2].TryPlace(telur));
            Assert.IsTrue(plate.slots[3].TryPlace(pisang));
            yield return null;

            Assert.IsFalse(plate.CanServe(out string reason), "A plate without vegetables must not be serveable.");
            StringAssert.Contains("Tambahkan 1 sayur", reason);
        }

        [UnityTest]
        public IEnumerator FriedFoodShowsWarningAndOnlyRetry()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "Plate missing.");
            Assert.IsNotNull(flow, "Flow missing.");

            var burger = foods.First(food => food.data != null && food.data.id == "burger");
            var friedEgg = foods.First(food => food.data != null && food.data.id == "telur_ceplok");
            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");
            var pisang = foods.First(food => food.data != null && food.data.id == "pisang");

            Assert.IsTrue(plate.slots[0].TryPlace(burger));
            Assert.IsTrue(plate.slots[1].TryPlace(friedEgg));
            Assert.IsTrue(plate.slots[2].TryPlace(brokoli));
            Assert.IsTrue(plate.slots[3].TryPlace(pisang));

            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(flow.IsReportOpen, "The warning should block the report flow.");
            Assert.IsNotNull(flow.friedWarning, "Fried warning presenter missing.");
            Assert.IsTrue(flow.friedWarning.root.activeSelf, "The fried warning panel should be visible.");
            Assert.IsFalse(flow.finalResult.root.activeSelf, "Final Result must not show for a fried plate.");
            Assert.AreEqual(flow.friedWarning.retryButton.gameObject, EventSystem.current.currentSelectedGameObject,
                "The retry button should receive focus.");
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

            var burger = foods.First(food => food.data != null && food.data.id == "burger");
            var rendang = foods.First(food => food.data != null && food.data.id == "rendang");
            var brokoli = foods.First(food => food.data != null && food.data.id == "brokoli");
            var pisang = foods.First(food => food.data != null && food.data.id == "pisang");

            Assert.IsTrue(plate.slots[0].TryPlace(burger));
            Assert.IsTrue(plate.slots[1].TryPlace(rendang));
            Assert.IsTrue(plate.slots[2].TryPlace(brokoli));
            Assert.IsTrue(plate.slots[3].TryPlace(pisang));

            flow.TryServe();
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(flow.IsReportOpen, "Report should open.");
            Assert.IsTrue(flow.finalResult.root.activeSelf, "Final Result should be visible.");
            Assert.IsFalse(flow.friedWarning.root.activeSelf, "Fried warning must stay hidden.");
        }
    }
}
