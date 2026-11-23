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

            var grid = Object.FindFirstObjectByType<FoodCardGridPresenter>();
            Assert.IsNotNull(grid, "FoodCardGridPresenter missing.");
            Assert.AreEqual(8, grid.CardCount, "Food card grid should contain 8 cards.");

            var first = grid.Cards[0];
            Assert.IsNotNull(first, "First food card missing.");
            Assert.AreEqual("rendang", first.FoodId);
            Assert.AreEqual("Rendang", first.nameText.text);
            Assert.AreEqual("PROTEIN", first.categoryText.text);
            Assert.IsNull(first.transform.Find("Calories"), "Nutrition rows must be removed from the card.");
        }

        [UnityTest]
        public IEnumerator FoodCardFocusAndStatusUpdate()
        {
            SceneManager.LoadScene("MainVR");
            yield return null;
            yield return null;

            var grid = Object.FindFirstObjectByType<FoodCardGridPresenter>();
            var status = Object.FindFirstObjectByType<MealStatusBarPresenter>();
            var plate = Object.FindFirstObjectByType<PlateController>();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None);
            Assert.IsNotNull(grid, "FoodCardGridPresenter missing.");
            Assert.IsNotNull(status, "MealStatusBarPresenter missing.");
            Assert.IsNotNull(plate, "PlateController missing.");
            Assert.GreaterOrEqual(foods.Length, 3, "Not enough food.");

            grid.Focus("tempe");
            var tempeCard = FindCard(grid, "tempe");
            Assert.IsNotNull(tempeCard, "Tempe card missing.");
            Assert.IsTrue(tempeCard.focusOutline.activeSelf, "Focused card outline should be active.");

            Assert.AreEqual("BELUM DINILAI", status.statusLabel.text);
            var picks = new[] { "burger", "rendang", "brokoli", "pisang" }
                .Select(id => foods.First(food => food.data != null && food.data.id == id))
                .ToArray();
            for (int i = 0; i < picks.Length; i++)
                Assert.IsTrue(plate.slots[i].TryPlace(picks[i]), "Could not place " + picks[i].data.id);

            yield return null;
            Assert.AreNotEqual("BELUM DINILAI", status.statusLabel.text);
            Assert.Greater(status.fill.fillAmount, 0f);
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
