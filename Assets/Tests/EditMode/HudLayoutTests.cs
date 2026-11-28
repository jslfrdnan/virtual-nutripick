using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NutriPick.Tests
{
    public class HudLayoutTests
    {
        [Test]
        public void CanvasUsesTheEnlargedReferenceResolution()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FigmaHUD.prefab");
            Assert.IsNotNull(hud, "FigmaHUD prefab missing.");
            var scaler = hud.GetComponent<CanvasScaler>();
            Assert.IsNotNull(scaler, "CanvasScaler missing.");
            Assert.AreEqual(new Vector2(1600f, 900f), scaler.referenceResolution);
        }

        [Test]
        public void FoodCardAndGridUseTheTwoColumnLayout()
        {
            var card = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FoodCard.prefab");
            var grid = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FoodCardGrid.prefab");
            Assert.IsNotNull(card, "FoodCard prefab missing.");
            Assert.IsNotNull(grid, "FoodCardGrid prefab missing.");

            Assert.AreEqual(new Vector2(180f, 82f), card.GetComponent<RectTransform>().sizeDelta);
            var layout = grid.GetComponentInChildren<GridLayoutGroup>();
            Assert.IsNotNull(layout, "GridLayoutGroup missing.");
            Assert.AreEqual(new Vector2(180f, 82f), layout.cellSize);
            Assert.AreEqual(2, layout.constraintCount);

            int rows = Mathf.CeilToInt(18f / layout.constraintCount);
            float height = layout.padding.top + layout.padding.bottom +
                rows * layout.cellSize.y + (rows - 1) * layout.spacing.y;
            Assert.LessOrEqual(height, 900f, "Eighteen cards must fit the 900px reference height.");
        }

        [Test]
        public void StatusBarUsesTheEnlargedSize()
        {
            var status = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/StatusMakanan.prefab");
            Assert.IsNotNull(status, "StatusMakanan prefab missing.");
            Assert.AreEqual(new Vector2(700f, 84f), status.GetComponent<RectTransform>().sizeDelta);
        }

        [Test]
        public void PauseMenuButtonsStayInsideThePanel()
        {
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FigmaHUD.prefab");
            var pause = hud.transform.Find("PausePanel");
            Assert.IsNotNull(pause, "PausePanel missing.");
            var panel = pause.Find("Panel") as RectTransform;
            Assert.IsNotNull(panel, "Pause panel card missing.");
            Assert.AreEqual(new Vector2(620f, 460f), panel.sizeDelta);

            foreach (var name in new[] { "ResumeButton", "HelpButton", "QuitButton" })
            {
                var button = panel.Find(name) as RectTransform;
                Assert.IsNotNull(button, $"{name} missing.");
                float top = button.anchoredPosition.y + button.sizeDelta.y * 0.5f;
                float bottom = button.anchoredPosition.y - button.sizeDelta.y * 0.5f;
                Assert.LessOrEqual(Mathf.Abs(top), panel.sizeDelta.y * 0.5f, $"{name} pokes above the panel.");
                Assert.LessOrEqual(Mathf.Abs(bottom), panel.sizeDelta.y * 0.5f, $"{name} pokes below the panel.");
            }
        }
    }
}
