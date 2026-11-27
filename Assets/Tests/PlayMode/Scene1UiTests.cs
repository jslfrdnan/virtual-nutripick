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
    public class Scene1UiTests
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
        public IEnumerator FoodCardGridBindsEightCards()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            TestQuestUtility.StartLevel(flow);
            var grid = Object.FindFirstObjectByType<FoodCardGridPresenter>();
            Assert.IsNotNull(grid, "FoodCardGridPresenter missing.");
            Assert.AreEqual(8, grid.CardCount, "Food card grid should contain 8 cards.");

            var first = grid.Cards[0];
            Assert.IsNotNull(first, "First food card missing.");
            Assert.AreEqual("nasi_putih", first.FoodId);
            Assert.AreEqual("Nasi putih", first.nameText.text);
            Assert.AreEqual("KARBOHIDRAT", first.categoryText.text);
            Assert.IsNull(first.transform.Find("Calories"), "Nutrition rows must be removed from the card.");
        }

        [UnityTest]
        public IEnumerator FoodCardFocusAndStatusUpdate()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var plate = Object.FindFirstObjectByType<PlateController>();
            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.GreaterOrEqual(foods.Length, 5, "Not enough food.");
            TestQuestUtility.StartLevel(flow);

            var grid = Object.FindFirstObjectByType<FoodCardGridPresenter>();
            var status = Object.FindFirstObjectByType<MealStatusBarPresenter>();
            Assert.IsNotNull(grid, "FoodCardGridPresenter missing.");
            Assert.IsNotNull(status, "MealStatusBarPresenter missing.");

            grid.Focus("tahu");
            var tahuCard = FindCard(grid, "tahu");
            Assert.IsNotNull(tahuCard, "Tahu card missing.");
            Assert.IsTrue(tahuCard.focusOutline.activeSelf, "Focused card outline should be active.");

            StringAssert.Contains("ANAK-ANAK", status.statusLabel.text);
            StringAssert.Contains("SARAPAN", status.statusLabel.text);
            var picks = new[] { "tahu", "telur_rebus", "pisang", "brokoli", "nasi_putih" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            yield return null;
            StringAssert.Contains("PROTEIN [x]", status.countsLabel.text);
            StringAssert.Contains("BUAH [x]", status.countsLabel.text);
            StringAssert.Contains("KALSIUM", status.countsLabel.text);
        }

        private static FoodCardView FindCard(FoodCardGridPresenter grid, string foodId)
        {
            foreach (var card in grid.Cards)
            {
                if (card != null && card.FoodId == foodId)
                    return card;
            }
            return null;
        }
    }
}
