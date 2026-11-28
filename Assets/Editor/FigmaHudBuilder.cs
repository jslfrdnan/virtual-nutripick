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
            var scaler = root.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.referenceResolution = new Vector2(1600f, 900f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            var font = FindFont(root);
            var placeholder = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderPath);

            var cardPrefab = BuildFoodCardPrefab(font, placeholder);
            var gridPrefab = BuildFoodCardGridPrefab(font, cardPrefab, placeholder);
            var statusPrefab = BuildStatusMakananPrefab(font);
            var finalResultPrefab = BuildFinalResultPrefab(font);

            DestroyChild(root.transform, "StartScreen");
            DestroyChild(root.transform, "StartGuide");
            DestroyChild(root.transform, "QuestSelect");
            DestroyChild(root.transform, "CampaignSummary");
            DestroyChild(root.transform, "FriedWarning");
            DestroyChild(root.transform, "ReportRoot");
            DestroyChild(root.transform, "ReportController");
            DestroyChild(root.transform, "SprintIndicator");
            DestroyChild(root.transform, "FoodCardGrid");
            DestroyChild(root.transform, "StatusMakanan");
            DestroyChild(root.transform, "FinalResult");

            var grid = (GameObject)PrefabUtility.InstantiatePrefab(gridPrefab, root.transform);
            grid.name = "FoodCardGrid";
            var gridRect = grid.GetComponent<RectTransform>();
            PlaceAnchored(gridRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 55f), new Vector2(634f, 760f));
            gridRect.pivot = new Vector2(1f, 0.5f);
            gridRect.anchoredPosition = new Vector2(-24f, 55f);

            var status = (GameObject)PrefabUtility.InstantiatePrefab(statusPrefab, root.transform);
            status.name = "StatusMakanan";
            var statusRect = status.GetComponent<RectTransform>();
            PlaceAnchored(statusRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(760f, 68f));
            statusRect.pivot = new Vector2(0.5f, 0f);
            statusRect.anchoredPosition = new Vector2(0f, 24f);

            var finalResult = (GameObject)PrefabUtility.InstantiatePrefab(finalResultPrefab, root.transform);
            finalResult.name = "FinalResult";
            Stretch(finalResult.GetComponent<RectTransform>());

            BuildStartGuidePanel(root.transform, font);
            BuildQuestSelectPanel(root.transform, font);
            BuildCampaignSummaryPanel(root.transform, font);
            BuildSprintIndicator(root.transform, font);
            ConfigurePauseMenu(root.transform);
            AdjustInfoCard(root.transform);

            DestroyChild(root.transform, "NutritionBar");
            SetActive(root.transform, "InfoCard", false);

            var prompt = FindDeepChild(root.transform, "Prompt") as RectTransform;
            if (prompt != null)
            {
                prompt.anchoredPosition = new Vector2(0f, 120f);
                prompt.sizeDelta = new Vector2(680f, 48f);
                var promptText = prompt.GetComponentInChildren<TMP_Text>(true);
                if (promptText != null)
                    promptText.fontSize = 26f;
            }

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
            rect.sizeDelta = new Vector2(190f, 100f);

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
            PlaceTopLeft(iconBackground.rectTransform, new Vector2(12f, -12f), new Vector2(58f, 58f));
            iconBackground.raycastTarget = false;

            var icon = CreateImage("Icon", root.transform, Color.white);
            PlaceTopLeft(icon.rectTransform, new Vector2(12f, -12f), new Vector2(58f, 58f));
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            view.iconImage = icon;

            view.nameText = CreateText("Name", root.transform, "Food", 19f, new Color(0.16f, 0.16f, 0.16f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.nameText.rectTransform, new Vector2(80f, -14f), new Vector2(100f, 26f));

            view.categoryText = CreateText("Category", root.transform, "KARBOHIDRAT", 13f, new Color(0.18f, 0.49f, 0.20f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(view.categoryText.rectTransform, new Vector2(80f, -42f), new Vector2(100f, 18f));

            PrefabUtility.SaveAsPrefabAsset(root, FoodCardPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(FoodCardPath);
        }

        private static GameObject BuildFoodCardGridPrefab(TMP_FontAsset font, GameObject cardPrefab, Sprite placeholder)
        {
            var root = new GameObject("FoodCardGrid", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(FoodCardGridPresenter));
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(634f, 760f);
            var background = root.GetComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.09f, 0.78f);
            background.raycastTarget = false;

            var container = new GameObject("Cards", typeof(RectTransform));
            var containerRect = container.GetComponent<RectTransform>();
            containerRect.SetParent(root.transform, false);
            Stretch(containerRect, 8f, 8f, 8f, 8f);
            var layout = container.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(190f, 100f);
            layout.spacing = new Vector2(12f, 12f);
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
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
            rect.sizeDelta = new Vector2(760f, 68f);
            var background = root.GetComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.09f, 0.85f);
            background.raycastTarget = false;

            var presenter = root.GetComponent<MealStatusBarPresenter>();
            presenter.statusLabel = CreateText("StatusLabel", root.transform, "ANAK-ANAK · SARAPAN", 19f, Color.white, TextAlignmentOptions.Left, font);
            PlaceTopLeft(presenter.statusLabel.rectTransform, new Vector2(16f, -8f), new Vector2(460f, 26f));

            presenter.countsLabel = CreateText("CountsLabel", root.transform, "PROTEIN [ ]  BUAH [ ]  PROTEIN 0/15 g [ ]  KALSIUM 0/330 mg [ ]", 13f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(presenter.countsLabel.rectTransform, new Vector2(16f, -40f), new Vector2(728f, 20f));

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
            presenter.titleText = CreateText("Title", root.transform, "HASIL LEVEL", 48f, Color.white, TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 350f), new Vector2(900f, 70f));

            presenter.subtitleText = CreateText("Subtitle", root.transform, "Quest · Level", 20f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.subtitleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 305f), new Vector2(900f, 30f));

            var taskPanel = CreateImage("TaskPanel", root.transform, new Color(0.10f, 0.12f, 0.13f, 1f));
            PlaceAnchored(taskPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300f, -5f), new Vector2(520f, 420f));
            taskPanel.raycastTarget = false;

            presenter.taskText = CreateText("TaskText", taskPanel.transform, "TASK", 20f, new Color(0.92f, 0.92f, 0.88f), TextAlignmentOptions.TopLeft, font);
            Stretch(presenter.taskText.rectTransform, 24f, 20f, 24f, 20f);
            presenter.taskText.textWrappingMode = TextWrappingModes.Normal;

            var nutritionPanel = CreateImage("NutritionPanel", root.transform, new Color(0.10f, 0.12f, 0.13f, 1f));
            PlaceAnchored(nutritionPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, -5f), new Vector2(520f, 420f));
            nutritionPanel.raycastTarget = false;

            presenter.nutritionText = CreateText("NutritionText", nutritionPanel.transform, "GIZI PIRING", 18f, new Color(0.92f, 0.92f, 0.88f), TextAlignmentOptions.TopLeft, font);
            Stretch(presenter.nutritionText.rectTransform, 24f, 20f, 24f, 20f);
            presenter.nutritionText.textWrappingMode = TextWrappingModes.Normal;

            presenter.warningText = CreateText("Warning", root.transform, string.Empty, 16f, new Color(0.30f, 0.69f, 0.31f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.warningText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -250f), new Vector2(1000f, 44f));
            presenter.warningText.textWrappingMode = TextWrappingModes.Normal;

            presenter.closingText = CreateText("Closing", root.transform, "PILIHAN KECIL, DAMPAK BESAR!", 22f, new Color(0.18f, 0.49f, 0.20f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.closingText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -288f), new Vector2(900f, 30f));

            presenter.disclaimerText = CreateText("Disclaimer", root.transform, "Game edukasi, bukan nasihat medis. Nilai gizi adalah perkiraan.", 12f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.disclaimerText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -318f), new Vector2(900f, 24f));

            presenter.retryButton = CreateButton("RetryButton", root.transform, "ULANGI LEVEL", new Vector2(-160f, -370f), new Vector2(280f, 60f), new Color(0.30f, 0.69f, 0.31f), Color.white, font);
            presenter.nextButton = CreateButton("NextButton", root.transform, "NEXT LEVEL", new Vector2(160f, -370f), new Vector2(280f, 60f), new Color(0.30f, 0.69f, 0.31f), Color.white, font);

            presenter.inputHint = CreateText("InputHint", root.transform, "D-pad / left stick: pilih · A / Enter: konfirmasi", 16f, new Color(0.78f, 0.82f, 0.84f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.inputHint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -415f), new Vector2(900f, 24f));

            ConfigureFocusHighlight(presenter.retryButton);
            ConfigureFocusHighlight(presenter.nextButton);
            ConfigureButtonNavigation(presenter.retryButton, presenter.nextButton);

            root.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, FinalResultPath);
            Object.DestroyImmediate(root);
            return AssetDatabase.LoadAssetAtPath<GameObject>(FinalResultPath);
        }

        private static void BuildStartGuidePanel(Transform parent, TMP_FontAsset font)
        {
            var panel = new GameObject("StartGuide", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(StartGuidePresenter));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            Stretch(panelRect);
            var panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.02f, 0.03f, 0.04f, 1f);
            panelImage.raycastTarget = true;

            var card = CreateImage("Card", panel.transform, new Color(0.07f, 0.09f, 0.11f, 0.98f));
            PlaceAnchored(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1040f, 620f));

            var title = CreateText("Title", card.transform, "CARA BERMAIN", 42f, new Color(1f, 0.78f, 0.22f), TextAlignmentOptions.Center, font);
            PlaceAnchored(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 265f), new Vector2(900f, 60f));

            var controls = CreateText(
                "Controls",
                card.transform,
                "KONTROL\n" +
                "Left stick / WASD: bergerak\n" +
                "Right stick / mouse: melihat\n" +
                "A / Enter: ambil, taruh, konfirmasi\n" +
                "B / klik kanan: lepaskan\n" +
                "X / Tab: lihat info makanan\n" +
                "Start / Space: sajikan\n" +
                "RT: toggle sprint 2x\n" +
                "Select / Esc: pause\n" +
                "D-pad: navigasi menu",
                18f,
                new Color(0.92f, 0.92f, 0.88f),
                TextAlignmentOptions.TopLeft,
                font);
            PlaceAnchored(controls.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-245f, 20f), new Vector2(450f, 320f));

            var rules = CreateText(
                "Rules",
                card.transform,
                "ATURAN\n" +
                "1. Pilih kelompok: anak, dewasa, atau lansia.\n" +
                "2. Tiap level punya 4 task: 2 komposisi + 2 ambang gizi.\n" +
                "3. Serve aktif hanya jika keempat task terpenuhi.\n" +
                "4. Batas natrium, gula, dan lemak ditandai di report.\n" +
                "5. Tekan X / Tab untuk melihat gizi lengkap makanan.",
                18f,
                new Color(0.92f, 0.92f, 0.88f),
                TextAlignmentOptions.TopLeft,
                font);
            PlaceAnchored(rules.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(245f, 30f), new Vector2(450f, 320f));

            var quest = CreateText(
                "QuestSummary",
                card.transform,
                "QUEST AKTIF\n—",
                14f,
                new Color(0.72f, 0.76f, 0.78f),
                TextAlignmentOptions.TopLeft,
                font);
            PlaceAnchored(quest.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -205f), new Vector2(900f, 60f));

            var start = CreateButton("StartButton", card.transform, "MULAI", new Vector2(0f, -265f), new Vector2(260f, 60f), new Color(0.30f, 0.69f, 0.31f), Color.white, font);
            ConfigureFocusHighlight(start);

            var presenter = panel.GetComponent<StartGuidePresenter>();
            presenter.root = panel;
            presenter.controlsText = controls;
            presenter.rulesText = rules;
            presenter.questText = quest;
            presenter.startButton = start;
            panel.SetActive(false);
        }

        private static void BuildQuestSelectPanel(Transform parent, TMP_FontAsset font)
        {
            var panel = new GameObject("QuestSelect", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(QuestSelectPresenter));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            Stretch(panelRect);
            var panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.02f, 0.03f, 0.04f, 1f);
            panelImage.raycastTarget = true;

            var card = CreateImage("Card", panel.transform, new Color(0.07f, 0.09f, 0.11f, 0.98f));
            PlaceAnchored(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1140f, 660f));

            var title = CreateText("Title", card.transform, "PILIH KELOMPOK", 42f, new Color(1f, 0.78f, 0.22f), TextAlignmentOptions.Center, font);
            PlaceAnchored(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 275f), new Vector2(900f, 60f));

            var subtitle = CreateText("Subtitle", card.transform, "Quest menentukan task gizi tiap level.", 16f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Center, font);
            PlaceAnchored(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 235f), new Vector2(900f, 26f));

            var presenter = panel.GetComponent<QuestSelectPresenter>();
            presenter.root = panel;
            presenter.questButtons = new Button[3];
            presenter.questTitles = new TMP_Text[3];
            presenter.questDetails = new TMP_Text[3];

            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 360f;
                var cardObject = new GameObject("Quest" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                var rect = cardObject.GetComponent<RectTransform>();
                rect.SetParent(card.transform, false);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(x, -35f);
                rect.sizeDelta = new Vector2(330f, 440f);

                var image = cardObject.GetComponent<Image>();
                image.color = new Color(0.10f, 0.12f, 0.13f, 1f);

                var button = cardObject.GetComponent<Button>();
                button.targetGraphic = image;
                var colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 0.92f, 0.70f);
                colors.selectedColor = new Color(1f, 0.92f, 0.70f);
                colors.pressedColor = new Color(0.85f, 0.80f, 0.65f);
                colors.fadeDuration = 0.1f;
                button.colors = colors;
                ConfigureFocusHighlight(button);

                var cardTitle = CreateText("Title", cardObject.transform, "KELOMPOK", 24f, new Color(1f, 0.78f, 0.22f), TextAlignmentOptions.Center, font);
                PlaceAnchored(cardTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 185f), new Vector2(300f, 40f));

                var details = CreateText("Details", cardObject.transform, string.Empty, 13f, new Color(0.92f, 0.92f, 0.88f), TextAlignmentOptions.TopLeft, font);
                PlaceAnchored(details.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -15f), new Vector2(300f, 370f));
                details.textWrappingMode = TextWrappingModes.Normal;

                presenter.questButtons[i] = button;
                presenter.questTitles[i] = cardTitle;
                presenter.questDetails[i] = details;
            }

            panel.SetActive(false);
        }

        private static void BuildCampaignSummaryPanel(Transform parent, TMP_FontAsset font)
        {
            var panel = new GameObject("CampaignSummary", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CampaignSummaryPresenter));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            Stretch(panelRect);
            var panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.06f, 0.08f, 0.09f, 0.96f);
            panelImage.raycastTarget = true;

            var card = CreateImage("Card", panel.transform, new Color(0.07f, 0.09f, 0.11f, 0.98f));
            PlaceAnchored(card.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1140f, 700f));

            var presenter = panel.GetComponent<CampaignSummaryPresenter>();
            presenter.root = panel;
            presenter.titleText = CreateText("Title", card.transform, "RINGKASAN KAMPANYE", 42f, new Color(1f, 0.78f, 0.22f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 295f), new Vector2(900f, 60f));

            presenter.subtitleText = CreateText("Subtitle", card.transform, "Quest", 18f, new Color(0.72f, 0.76f, 0.78f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.subtitleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 250f), new Vector2(900f, 28f));

            var levelsPanel = CreateImage("LevelsPanel", card.transform, new Color(0.10f, 0.12f, 0.13f, 1f));
            PlaceAnchored(levelsPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-280f, -20f), new Vector2(510f, 470f));
            levelsPanel.raycastTarget = false;

            presenter.levelsText = CreateText("LevelsText", levelsPanel.transform, "HASIL PER LEVEL", 20f, new Color(0.92f, 0.92f, 0.88f), TextAlignmentOptions.TopLeft, font);
            Stretch(presenter.levelsText.rectTransform, 24f, 20f, 24f, 20f);

            var totalsPanel = CreateImage("TotalsPanel", card.transform, new Color(0.10f, 0.12f, 0.13f, 1f));
            PlaceAnchored(totalsPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(280f, -20f), new Vector2(510f, 470f));
            totalsPanel.raycastTarget = false;

            presenter.totalsText = CreateText("TotalsText", totalsPanel.transform, "TOTAL HARIAN", 16f, new Color(0.92f, 0.92f, 0.88f), TextAlignmentOptions.TopLeft, font);
            Stretch(presenter.totalsText.rectTransform, 24f, 20f, 24f, 20f);

            presenter.warningText = CreateText("Warning", card.transform, string.Empty, 14f, new Color(0.30f, 0.69f, 0.31f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.warningText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -285f), new Vector2(1000f, 40f));
            presenter.warningText.textWrappingMode = TextWrappingModes.Normal;

            presenter.restartButton = CreateButton("RestartButton", card.transform, "MAIN LAGI", new Vector2(0f, -345f), new Vector2(280f, 60f), new Color(0.30f, 0.69f, 0.31f), Color.white, font);
            ConfigureFocusHighlight(presenter.restartButton);

            presenter.inputHint = CreateText("InputHint", card.transform, "A / Enter: main lagi", 16f, new Color(0.78f, 0.82f, 0.84f), TextAlignmentOptions.Center, font);
            PlaceAnchored(presenter.inputHint.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -395f), new Vector2(900f, 24f));

            panel.SetActive(false);
        }

        private static void AdjustInfoCard(Transform root)
        {
            var stats = FindDeepChild(root, "FullStats");
            if (stats != null)
            {
                var rect = stats as RectTransform;
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(380f, 230f);
                    rect.anchoredPosition = new Vector2(0f, 20f);
                }

                var text = stats.GetComponent<TMP_Text>();
                if (text != null)
                    text.fontSize = 15f;
            }

            var description = FindDeepChild(root, "FullDescription");
            if (description != null)
            {
                var rect = description as RectTransform;
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(380f, 100f);
                    rect.anchoredPosition = new Vector2(0f, -175f);
                }

                var text = description.GetComponent<TMP_Text>();
                if (text != null)
                    text.fontSize = 14f;
            }
        }

        private static void BuildSprintIndicator(Transform parent, TMP_FontAsset font)
        {
            var label = CreateText("SprintIndicator", parent, "SPRINT ON", 17f, new Color(1f, 0.78f, 0.22f), TextAlignmentOptions.Left, font);
            PlaceTopLeft(label.rectTransform, new Vector2(24f, -24f), new Vector2(190f, 28f));
            label.gameObject.SetActive(false);
        }

        private static void ConfigurePauseMenu(Transform root)
        {
            var pause = FindDeepChild(root, "PausePanel");
            if (pause == null)
                return;

            var resume = FindDeepChild(pause, "ResumeButton")?.GetComponent<Button>();
            var quit = FindDeepChild(pause, "QuitButton")?.GetComponent<Button>();
            var help = FindDeepChild(pause, "HelpButton")?.GetComponent<Button>();

            if (resume == null || quit == null)
                return;

            if (help == null)
            {
                var duplicate = Object.Instantiate(resume.gameObject, resume.transform.parent);
                duplicate.name = "HelpButton";
                var label = FindDeepChild(duplicate.transform, "Label")?.GetComponent<TMP_Text>();
                if (label != null)
                    label.text = "BANTUAN";
                help = duplicate.GetComponent<Button>();
            }

            PlaceAnchored(resume.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(220f, 56f));
            PlaceAnchored(help.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(220f, 56f));
            PlaceAnchored(quit.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -180f), new Vector2(220f, 56f));

            ConfigureFocusHighlight(resume);
            ConfigureFocusHighlight(help);
            ConfigureFocusHighlight(quit);
            ConfigurePauseNavigation(resume, null, help);
            ConfigurePauseNavigation(help, resume, quit);
            ConfigurePauseNavigation(quit, help, null);
        }

        private static void ConfigurePauseNavigation(Button target, Button up, Button down)
        {
            if (target == null)
                return;

            var navigation = target.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = up != null ? up : target;
            navigation.selectOnDown = down != null ? down : target;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = null;
            target.navigation = navigation;
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
