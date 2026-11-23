using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class FigmaHudBuilder
    {
        private const string SourceHudPath = "Assets/Prefabs/UI/HUD.prefab";
        private const string TargetHudPath = "Assets/Prefabs/UI/FigmaHUD.prefab";
        private const string FoodCardPath = "Assets/Prefabs/UI/FoodCard.prefab";
        private const string FoodCardGridPath = "Assets/Prefabs/UI/FoodCardGrid.prefab";
        private const string StatusMakananPath = "Assets/Prefabs/UI/StatusMakanan.prefab";
        private const string FinalResultPath = "Assets/Prefabs/UI/FinalResult.prefab";
        private const string PlaceholderPath = "Assets/Textures/FoodIcons/PlaceholderFood.png";

        [MenuItem("NutriPick/Build Figma HUD")]
        public static void Build()
        {
            var root = PrefabUtility.LoadPrefabContents(SourceHudPath);
            if (root == null)
            {
                Debug.LogError($"[FigmaHUD] Missing {SourceHudPath}");
                return;
            }

            root.name = "FigmaHUD";
            var font = FindFont(root);
            var placeholder = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderPath);

            var cardPrefab = BuildFoodCardPrefab(font, placeholder);
            var gridPrefab = BuildFoodCardGridPrefab(font, cardPrefab, placeholder);
            var statusPrefab = BuildStatusMakananPrefab(font);
            var finalResultPrefab = BuildFinalResultPrefab(font);

            DestroyChild(root.transform, "StartScreen");
            DestroyChild(root.transform, "FoodCardGrid");
            DestroyChild(root.transform, "StatusMakanan");
            DestroyChild(root.transform, "FinalResult");

            var grid = (GameObject)PrefabUtility.InstantiatePrefab(gridPrefab, root.transform);
            grid.name = "FoodCardGrid";
            var gridRect = grid.GetComponent<RectTransform>();
            PlaceAnchored(gridRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(420f, 760f));
            gridRect.pivot = new Vector2(1f, 0.5f);
            gridRect.anchoredPosition = new Vector2(-24f, 0f);

            var status = (GameObject)PrefabUtility.InstantiatePrefab(statusPrefab, root.transform);
            status.name = "StatusMakanan";
            var statusRect = status.GetComponent<RectTransform>();
            PlaceAnchored(statusRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(720f, 80f));
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 24f);

            var finalResult = (GameObject)PrefabUtility.InstantiatePrefab(finalResultPrefab, root.transform);
            finalResult.name = "FinalResult";
            Stretch(finalResult.GetComponent<RectTransform>());

            SetActive(root.transform, "NutritionBar", false);
            SetActive(root.transform, "InfoCard", false);

            var prompt = FindDeepChild(root.transform, "Prompt") as RectTransform;
            if (prompt != null)
                prompt.anchoredPosition = new Vector2(0f, 120f);

            PrefabUtility.SaveAsPrefabAsset(root, TargetHudPath);
            PrefabUtility.UnloadPrefabContents(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FigmaHUD] Created {TargetHudPath}, {FoodCardPath}, {FoodCardGridPath}, {StatusMakananPath}, {FinalResultPath}");
        }

        private static GameObject BuildFoodCardPrefab(TMP_FontAsset font, Sprite placeholder)
        {
            var root = new GameObject("FoodCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FoodCardView));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(180f, 130f);

            var background = root.GetComponent<Image>();
            background.color = new Color(1f, 1f, 1f, 0.96f);
            background.raycastTarget = false;

            var view = root.GetComponent<FoodCardView>();
            view.cardBackground = background;
            view.placeholderSprite = placeholder;

            var outline = CreateImage("FocusOutline", root.transform, new Color(0.18f, 0.49f, 0.20f, 0.28f));
            Stretch(outline.rectTransform);
            outline.raycastTarget = false;
            outline.gameObject.SetActive(false);
            view.focusOutline = outline.gameObject;

            var accent = CreateImage("HealthAccent", root.transform, new Color(0.18f, 0.49f, 0.20f));
            var accentRect = accent.rectTransform;
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.anchoredPosition = Vector2.zero;
            accentRect.sizeDelta = new Vector2(4f, 0f);
            accent.raycastTarget = false;
            view.healthAccent = accent;

            var iconBackground = CreateImage("IconBackground", root.transform, new Color(0.90f, 0.95f, 0.88f, 1f));
            PlaceTopLeft(iconBackground.rectTransform, new Vector2(12f, -12f), new Vector2(56f, 56f));
            iconBackground.raycastTarget = false;

            var icon = CreateImage("Icon", root.transform, Color.white);
            PlaceTopLeft(icon.rectTransform, new Vector2(12f, -12f), new Vector2(56f, 56f));
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            view.iconImage = icon;

            view.nameText = CreateText("Name", root.transform, "Food", 18f, new Color(0.16f, 0.16f, 0.16f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.nameText.rectTransform, new Vector2(78f, -14f), new Vector2(92f, 26f));

            view.proteinTypeText = CreateText("ProteinType", root.transform, "Nabati", 13f, new Color(0.18f, 0.49f, 0.20f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.proteinTypeText.rectTransform, new Vector2(78f, -40f), new Vector2(92f, 18f));

            view.caloriesText = CreateText("Calories", root.transform, "100 kkal", 16f, new Color(0.16f, 0.16f, 0.16f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.caloriesText.rectTransform, new Vector2(12f, -78f), new Vector2(156f, 20f));

            view.proteinText = CreateText("Protein", root.transform, "P 0 g", 11f, new Color(0.38f, 0.38f, 0.38f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.proteinText.rectTransform, new Vector2(12f, -102f), new Vector2(48f, 18f));

            view.carbsText = CreateText("Carbs", root.transform, "K 0 g", 11f, new Color(0.38f, 0.38f, 0.38f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.carbsText.rectTransform, new Vector2(62f, -102f), new Vector2(48f, 18f));

            view.fatText = CreateText("Fat", root.transform, "L 0 g", 11f, new Color(0.38f, 0.38f, 0.38f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.fatText.rectTransform, new Vector2(112f, -102f), new Vector2(50f, 18f));

            PrefabUtility.SaveAsPrefabAsset(root, FoodCardPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(FoodCardPath);
        }

        private static GameObject BuildFoodCardGridPrefab(TMP_FontAsset font, GameObject cardPrefab, Sprite placeholder)
        {
            var root = new GameObject("FoodCardGrid", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FoodCardGridPresenter));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(420f, 760f);
            var background = root.GetComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.09f, 0.78f);
            background.raycastTarget = false;

            var container = new GameObject("Cards", typeof(RectTransform));
            var containerRect = container.GetComponent<RectTransform>();
            containerRect.SetParent(root.transform, false);
            Stretch(containerRect, 8f, 8f, 8f, 8f);
            var layout = container.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(180f, 130f);
            layout.spacing = new Vector2(12f, 12f);
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 2;
            layout.childAlignment = TextAnchor.UpperCenter;

            var presenter = root.GetComponent<FoodCardGridPresenter>();
            presenter.cardContainer = containerRect;
            presenter.cardPrefab = cardPrefab.GetComponent<FoodCardView>();
            presenter.placeholderSprite = placeholder;

            PrefabUtility.SaveAsPrefabAsset(root, FoodCardGridPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(FoodCardGridPath);
        }

        private static GameObject BuildStatusMakananPrefab(TMP_FontAsset font)
        {
            var root = new GameObject("StatusMakanan", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MealStatusBarPresenter));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(720f, 80f);
            var background = root.GetComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.09f, 0.85f);
            background.raycastTarget = false;

            var presenter = root.GetComponent<MealStatusBarPresenter>();
            presenter.statusLabel = CreateText("StatusLabel", root.transform, "BELUM DINILAI", 20f, Color.white, TextAlignmentOptions.Left, font);
            PlaceTopLeft(presenter.statusLabel.rectTransform, new Vector2(16f, -8f), new Vector2(420f, 28f));

            presenter.detailLabel = CreateText("DetailLabel", root.transform, "Pilih makanan untuk mulai", 12f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(presenter.detailLabel.rectTransform, new Vector2(16f, -42f), new Vector2(420f, 18f));

            presenter.countsLabel = CreateText("CountsLabel", root.transform, "Sehat 0 · Kurang sehat 0", 12f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Right, font);
            var countsRect = presenter.countsLabel.rectTransform;
            countsRect.anchorMin = new Vector2(1f, 1f);
            countsRect.anchorMax = new Vector2(1f, 1f);
            countsRect.pivot = new Vector2(1f, 1f);
            countsRect.anchoredPosition = new Vector2(-16f, -42f);
            countsRect.sizeDelta = new Vector2(260f, 18f);

            var track = CreateImage("Track", root.transform, new Color(0.16f, 0.18f, 0.20f, 1f));
            var trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(0.5f, 0f);
            trackRect.anchorMax = new Vector2(0.5f, 0f);
            trackRect.pivot = new Vector2(0.5f, 0f);
            trackRect.anchoredPosition = new Vector2(0f, 8f);
            trackRect.sizeDelta = new Vector2(680f, 10f);
            track.raycastTarget = false;

            var fill = CreateImage("Fill", track.transform, new Color(0.18f, 0.49f, 0.20f));
            Stretch(fill.rectTransform);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0.5f;
            fill.raycastTarget = false;

            var marker = CreateImage("Marker", track.transform, Color.white);
            var markerRect = marker.rectTransform;
            markerRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.anchoredPosition = Vector2.zero;
            markerRect.sizeDelta = new Vector2(6f, 20f);
            marker.raycastTarget = false;

            presenter.fill = fill;
            presenter.marker = markerRect;
            PrefabUtility.SaveAsPrefabAsset(root, StatusMakananPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(StatusMakananPath);
        }

        private static GameObject BuildFinalResultPrefab(TMP_FontAsset font)
        {
            var root = new GameObject("FinalResult", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FinalResultPresenter));
            var rootRect = root.GetComponent<RectTransform>();
            Stretch(rootRect);
            var background = root.GetComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.09f, 0.94f);
            background.raycastTarget = true;

            var presenter = root.GetComponent<FinalResultPresenter>();
            presenter.root = root;
            presenter.titleText = CreateText("Title", root.transform, "FINAL RESULT", 48f, Color.white, TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 380f), new Vector2(900f, 70f));

            var balanced = CreateFinalResultPanel(
                root.transform,
                "BalancedPanel",
                "Pilihan yang mendukung tubuh seimbang",
                new Vector2(-300f, -30f),
                new Color(0.88f, 0.96f, 0.88f, 1f),
                new Color(0.18f, 0.49f, 0.20f),
                new Color(0.30f, 0.69f, 0.31f),
                font,
                out var balancedTitle,
                out var balancedPanel,
                out var balancedBadge,
                out var balancedBadgeBg,
                out var balancedPoints);

            var lessBalanced = CreateFinalResultPanel(
                root.transform,
                "LessBalancedPanel",
                "Pilihan kurang seimbang",
                new Vector2(300f, -30f),
                new Color(0.99f, 0.90f, 0.88f, 1f),
                new Color(0.78f, 0.16f, 0.16f),
                new Color(0.95f, 0.42f, 0.36f),
                font,
                out var lessBalancedTitle,
                out var lessBalancedPanel,
                out var lessBalancedBadge,
                out var lessBalancedBadgeBg,
                out var lessBalancedPoints);

            presenter.balancedTitleText = balancedTitle;
            presenter.balancedPanelBackground = balancedPanel;
            presenter.balancedBadgeText = balancedBadge;
            presenter.balancedBadgeBackground = balancedBadgeBg;
            presenter.balancedPointTexts = balancedPoints;
            presenter.lessBalancedTitleText = lessBalancedTitle;
            presenter.lessBalancedPanelBackground = lessBalancedPanel;
            presenter.lessBalancedBadgeText = lessBalancedBadge;
            presenter.lessBalancedBadgeBackground = lessBalancedBadgeBg;
            presenter.lessBalancedPointTexts = lessBalancedPoints;

            presenter.closingText = CreateText("Closing", root.transform, "PILIHAN KECIL, DAMPAK BESAR!", 24f, new Color(0.18f, 0.49f, 0.20f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.closingText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(900f, 34f));

            presenter.disclaimerText = CreateText("Disclaimer", root.transform, "Game edukasi, bukan nasihat medis. Nilai gizi adalah perkiraan.", 12f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.disclaimerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -340f), new Vector2(900f, 24f));

            presenter.retryButton = CreateButton("RetryButton", root.transform, "ULANGI LEVEL", new Vector2(-160f, -400f), new Vector2(280f, 60f), new Color(0.30f, 0.69f, 0.31f), Color.white, font);
            presenter.nextButton = CreateButton("NextButton", root.transform, "NEXT LEVEL", new Vector2(160f, -400f), new Vector2(280f, 60f), new Color(0.30f, 0.69f, 0.31f), Color.white, font);

            presenter.inputHint = CreateText("InputHint", root.transform, "D-pad / left stick: pilih · A / Enter: konfirmasi", 16f, new Color(0.78f, 0.82f, 0.84f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.inputHint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -455f), new Vector2(900f, 24f));

            ConfigureFocusHighlight(presenter.retryButton);
            ConfigureFocusHighlight(presenter.nextButton);
            ConfigureButtonNavigation(presenter.retryButton, presenter.nextButton);

            root.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, FinalResultPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(FinalResultPath);
        }

        private static GameObject CreateFinalResultPanel(
            Transform parent,
            string name,
            string titleValue,
            Vector2 position,
            Color background,
            Color titleColor,
            Color badgeColor,
            TMP_FontAsset font,
            out TMP_Text title,
            out Image panelBackground,
            out TMP_Text badge,
            out Image badgeBackground,
            out TMP_Text[] points)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = panel.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            PlaceAnchored(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(540f, 460f));
            panelBackground = panel.GetComponent<Image>();
            panelBackground.color = background;
            panelBackground.raycastTarget = false;

            title = CreateText("Title", panel.transform, titleValue, 24f, titleColor, TextAlignmentOptions.Left, font);
            PlaceAnchored(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(480f, 40f));

            badgeBackground = CreateImage("BadgeBg", panel.transform, badgeColor);
            PlaceAnchored(badgeBackground.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(200f, 34f));
            badgeBackground.raycastTarget = false;

            badge = CreateText("Badge", badgeBackground.transform, "LEBIH SEHAT", 16f, Color.white, TextAlignmentOptions.Center, font);
            Stretch(badge.rectTransform, 4f, 2f, 4f, 2f);

            points = new TMP_Text[3];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = CreateText("Point" + (i + 1), panel.transform, "—", 18f, new Color(0.20f, 0.20f, 0.20f), TextAlignmentOptions.Left, font);
                PlaceAnchored(points[i].rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 50f - i * 70f), new Vector2(460f, 60f));
                points[i].textWrappingMode = TextWrappingModes.Normal;
            }

            return panel;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            string text,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment,
            TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            if (font != null)
                label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            return label;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 position,
            Vector2 size,
            Color background,
            Color textColor,
            TMP_FontAsset font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            PlaceAnchored(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

            var image = go.GetComponent<Image>();
            image.color = background;
            image.raycastTarget = true;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText("Label", go.transform, label, 24f, textColor, TextAlignmentOptions.Center, font);
            Stretch(text.rectTransform, 8f, 4f, 8f, 4f);
            return button;
        }

        private static void ConfigureFocusHighlight(Button button)
        {
            if (button == null)
                return;

            if (button.GetComponent<SelectableFocusHighlight>() == null)
                button.gameObject.AddComponent<SelectableFocusHighlight>();

            var colors = button.colors;
            colors.normalColor = new Color(0.78f, 0.78f, 0.78f, 1f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
        }

        private static void ConfigureButtonNavigation(Button retry, Button next)
        {
            if (retry == null)
                return;

            var retryNavigation = retry.navigation;
            retryNavigation.mode = Navigation.Mode.Explicit;
            retryNavigation.selectOnLeft = null;
            retryNavigation.selectOnRight = next;
            retryNavigation.selectOnUp = null;
            retryNavigation.selectOnDown = null;
            retry.navigation = retryNavigation;

            if (next == null)
                return;

            var nextNavigation = next.navigation;
            nextNavigation.mode = Navigation.Mode.Explicit;
            nextNavigation.selectOnLeft = retry;
            nextNavigation.selectOnRight = null;
            nextNavigation.selectOnUp = null;
            nextNavigation.selectOnDown = null;
            next.navigation = nextNavigation;
        }

        private static TMP_FontAsset FindFont(GameObject root)
        {
            var sample = root.GetComponentInChildren<TextMeshProUGUI>(true);
            return sample != null ? sample.font : TMP_Settings.defaultFontAsset;
        }

        private static void PlaceAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void PlaceTopLeft(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetActive(Transform root, string name, bool active)
        {
            var child = FindDeepChild(root, name);
            if (child != null)
                child.gameObject.SetActive(active);
        }

        private static void DestroyChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;
            foreach (Transform child in parent)
            {
                var result = FindDeepChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
