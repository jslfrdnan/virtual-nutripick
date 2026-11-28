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
        public void FoodCardAndGridUseTheEnlargedThreeColumnLayout()
        {
            var card = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FoodCard.prefab");
            var grid = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/FoodCardGrid.prefab");
            Assert.IsNotNull(card, "FoodCard prefab missing.");
            Assert.IsNotNull(grid, "FoodCardGrid prefab missing.");

            Assert.AreEqual(new Vector2(190f, 100f), card.GetComponent<RectTransform>().sizeDelta);
            var layout = grid.GetComponentInChildren<GridLayoutGroup>();
            Assert.IsNotNull(layout, "GridLayoutGroup missing.");
            Assert.AreEqual(new Vector2(190f, 100f), layout.cellSize);
            Assert.AreEqual(3, layout.constraintCount);

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
            Assert.AreEqual(new Vector2(760f, 68f), status.GetComponent<RectTransform>().sizeDelta);
        }
    }
}
