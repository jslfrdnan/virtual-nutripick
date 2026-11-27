using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.Nutrition;
using NutriPick.NPC;
using NutriPick.Player;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class Scene3BazaarBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level03_Bazaar.unity";
        private const string LegacyScenePath = "Assets/Scenes/Scene 3.unity";
        private const string LevelPath = "Assets/ScriptableObjects/Levels/LD_Level03_Bazaar.asset";
        private const string RulesPath = "Assets/ScriptableObjects/Rules/NutritionRules.asset";
        private const string PlayerRigPath = "Assets/Prefabs/Player/PlayerRig.prefab";
        private const string FigmaHudPath = "Assets/Prefabs/UI/FigmaHUD.prefab";
        private const string ServingPlatePath = "Assets/Prefabs/Interaction/ServingPlate.prefab";
        private const string ServeStationPath = "Assets/Prefabs/Interaction/ServeStation.prefab";
        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private const string SchoolFbx = "Assets/Models/Environment/Bazaar/SchoolCampus.fbx";
        private const string StandFbx = "Assets/Models/Environment/Bazaar/BazaarStand.fbx";
        private const string ClosedStandFbx = "Assets/Models/Environment/Bazaar/BazaarStandClosed.fbx";
        private const string CafeFbx = "Assets/Models/Environment/Bazaar/CafeKiosk.fbx";
        private const string StageFbx = "Assets/Models/Environment/Bazaar/BazaarStage.fbx";
        private const string StreetPropsFbx = "Assets/Models/Environment/Bazaar/StreetProps.fbx";
        private const string PlateFbx = "Assets/Models/Food/Plating/Plate_Small.fbx";
        private const string BowlFbx = "Assets/Models/Food/Plating/Bowl_Small.fbx";
        private const string NpcFolder = "Assets/Prefabs/NPC";

        private const float StageX = -8f;
        private const float StageZ = 0f;
        private const float ServeZ = 3.65f;

        private static readonly Vector3 HealthyStandPosition = new(-14.5f, 0f, 28f);
        private const float HealthyStandYaw = 90f;
        private static readonly Vector3 JunkStandPosition = new(-3.5f, 0f, -23f);
        private const float JunkStandYaw = -90f;

        private static readonly string[] StallFoodOrder =
        {
            "nasi_putih", "telur_rebus", "tempe", "brokoli", "pisang", "tahu",
            "wortel", "jeruk", "ubi_jalar", "ikan_cakalang", "bayam", "rendang",
            "ayam_goreng", "telur_ceplok", "sayur_singkong", "burger", "mie_goreng", "kerupuk"
        };

        private struct StallPlacement
        {
            public readonly Vector3 position;
            public readonly float yaw;

            public StallPlacement(Vector3 position, float yaw)
            {
                this.position = position;
                this.yaw = yaw;
            }
        }

        private static readonly Vector3[] ClosedStandPositions =
        {
            new(-3.5f, 0f, 23f),
            new(-14.5f, 0f, 18f),
            new(-3.5f, 0f, 13f),
            new(-14.5f, 0f, -18f),
            new(-3.5f, 0f, -13f),
            new(-14.5f, 0f, -28f),
        };

        private static readonly float[] ClosedStandYaws = { -90f, 90f, -90f, 90f, -90f, 90f };

        private class MatSpec
        {
            public Color color;
            public float metallic;
            public float smoothness;
            public Color? emission;
        }

        private static readonly Dictionary<string, MatSpec> Palette = new()
        {
            { "M_Bazaar_WallCream", new MatSpec { color = new Color(0.93f, 0.89f, 0.80f), smoothness = 0.08f } },
            { "M_Bazaar_WallWarm", new MatSpec { color = new Color(0.86f, 0.78f, 0.64f), smoothness = 0.08f } },
            { "M_Bazaar_Terracotta", new MatSpec { color = new Color(0.62f, 0.30f, 0.18f), smoothness = 0.12f } },
            { "M_Bazaar_RoofDark", new MatSpec { color = new Color(0.28f, 0.22f, 0.18f), smoothness = 0.18f } },
            { "M_Bazaar_Wood", new MatSpec { color = new Color(0.55f, 0.38f, 0.22f), smoothness = 0.2f } },
            { "M_Bazaar_WoodDark", new MatSpec { color = new Color(0.32f, 0.21f, 0.12f), smoothness = 0.2f } },
            { "M_Bazaar_MetalDark", new MatSpec { color = new Color(0.06f, 0.06f, 0.06f), metallic = 0.7f, smoothness = 0.62f } },
            { "M_Bazaar_MetalLight", new MatSpec { color = new Color(0.55f, 0.55f, 0.52f), metallic = 1f, smoothness = 0.72f } },
            { "M_Bazaar_CanvasCream", new MatSpec { color = new Color(0.95f, 0.91f, 0.80f), smoothness = 0.05f } },
            { "M_Bazaar_CanvasRed", new MatSpec { color = new Color(0.72f, 0.16f, 0.12f), smoothness = 0.05f } },
            { "M_Bazaar_CanvasYellow", new MatSpec { color = new Color(0.92f, 0.68f, 0.16f), smoothness = 0.05f } },
            { "M_Bazaar_CanvasTeal", new MatSpec { color = new Color(0.12f, 0.42f, 0.40f), smoothness = 0.05f } },
            { "M_Bazaar_Leaf", new MatSpec { color = new Color(0.14f, 0.38f, 0.16f), smoothness = 0.1f } },
            { "M_Bazaar_LeafLight", new MatSpec { color = new Color(0.28f, 0.55f, 0.22f), smoothness = 0.1f } },
            { "M_Bazaar_Glass", new MatSpec { color = new Color(0.30f, 0.48f, 0.48f), smoothness = 0.9f } },
            { "M_Bazaar_Concrete", new MatSpec { color = new Color(0.62f, 0.60f, 0.55f), smoothness = 0.15f } },
            { "M_Bazaar_Paver", new MatSpec { color = new Color(0.76f, 0.64f, 0.50f), smoothness = 0.18f } },
            { "M_Bazaar_Bulb", new MatSpec { color = new Color(1f, 0.80f, 0.45f), smoothness = 0.3f, emission = new Color(1f, 0.62f, 0.25f) * 3f } },
            { "M_Bazaar_SignGreen", new MatSpec { color = new Color(0.06f, 0.24f, 0.14f), smoothness = 0.2f } },
            { "M_Bazaar_SignGold", new MatSpec { color = new Color(0.85f, 0.66f, 0.22f), metallic = 0.3f, smoothness = 0.45f } },
            { "M_Bazaar_FlagRed", new MatSpec { color = new Color(0.80f, 0.08f, 0.08f), smoothness = 0.05f } },
            { "M_Bazaar_FlagWhite", new MatSpec { color = new Color(0.95f, 0.95f, 0.92f), smoothness = 0.05f } },
            { "M_Bazaar_Chalkboard", new MatSpec { color = new Color(0.08f, 0.15f, 0.12f), smoothness = 0.05f } },
            { "M_Bazaar_Crate", new MatSpec { color = new Color(0.62f, 0.45f, 0.26f), smoothness = 0.1f } },
            { "M_Bazaar_ProduceOrange", new MatSpec { color = new Color(0.90f, 0.45f, 0.10f), smoothness = 0.2f } },
            { "M_Bazaar_ProduceGreen", new MatSpec { color = new Color(0.22f, 0.52f, 0.18f), smoothness = 0.2f } },
            { "M_Bazaar_ProduceRed", new MatSpec { color = new Color(0.70f, 0.12f, 0.10f), smoothness = 0.2f } },
            { "M_Bazaar_Bin", new MatSpec { color = new Color(0.10f, 0.24f, 0.16f), smoothness = 0.1f } },
            { "M_Bazaar_Asphalt", new MatSpec { color = new Color(0.085f, 0.085f, 0.095f), smoothness = 0.12f } },
            { "M_Bazaar_Sidewalk", new MatSpec { color = new Color(0.70f, 0.67f, 0.60f), smoothness = 0.12f } },
            { "M_Bazaar_Plaza", new MatSpec { color = new Color(0.76f, 0.62f, 0.46f), smoothness = 0.15f } },
            { "M_Bazaar_Grass", new MatSpec { color = new Color(0.18f, 0.32f, 0.14f), smoothness = 0.05f } },
            { "M_Bazaar_Curb", new MatSpec { color = new Color(0.84f, 0.81f, 0.73f), smoothness = 0.15f } },
            { "M_Bazaar_RoadLine", new MatSpec { color = new Color(0.92f, 0.89f, 0.80f), smoothness = 0.15f } },
            { "M_Home_Ceramic", new MatSpec { color = new Color(0.97f, 0.96f, 0.93f), smoothness = 0.35f } },
        };

        private static readonly Dictionary<string, Material> Materials = new();
        private static readonly HashSet<string> MissingMaterials = new();
        private static TMP_FontAsset font;

        [MenuItem("NutriPick/Build Scene 3 Bazaar")]
        public static void Build()
        {
            AssetDatabase.Refresh();

            if (File.Exists(LegacyScenePath))
                AssetDatabase.DeleteAsset(LegacyScenePath);

            ConfigureModelImporters();
            EnsureMaterials();
            UpdateLevel();
            BuildScene();
            ApplyBazaarDressing();
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            NutriPickSceneValidator.ValidateScenes();
            NutriPickSceneValidator.CaptureScenePreviews();

            Debug.Log("[Scene3Bazaar] Built Flamboyant bazaar: 30 open food stalls, 120 foods, 2 intersections, 4 coffee shops, school/SMK backdrop, and serving stage.");
        }

        private static void ApplyBazaarDressing()
        {
            BazaarFoodPolish.RunAll();
            BazaarEveningSetup.ApplyGoldenHour();
            BazaarStreetLamps.Build();
            BazaarGrassScatter.Build();
        }

        private static void ConfigureModelImporters()
        {
            var paths = new[]
            {
                SchoolFbx, StandFbx, ClosedStandFbx, CafeFbx, StageFbx, StreetPropsFbx
            };

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"[Scene3Bazaar] Missing FBX: {path}");
                    continue;
                }

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                    continue;

                importer.globalScale = 1f;
                importer.useFileScale = true;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importAnimation = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.importBlendShapes = false;
                importer.generateSecondaryUV = true;
                importer.meshCompression = ModelImporterMeshCompression.Off;
                importer.isReadable = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureMaterials()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials/Bazaar"))
                AssetDatabase.CreateFolder("Assets/Materials", "Bazaar");

            var litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                Debug.LogError("[Scene3Bazaar] URP Lit shader not found.");
                return;
            }

            foreach (var pair in Palette)
            {
                var path = $"Assets/Materials/Bazaar/{pair.Key}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(litShader);
                    AssetDatabase.CreateAsset(material, path);
                }

                material.shader = litShader;
                material.SetColor("_BaseColor", pair.Value.color);
                material.SetFloat("_Metallic", pair.Value.metallic);
                material.SetFloat("_Smoothness", pair.Value.smoothness);

                if (pair.Value.emission.HasValue)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", pair.Value.emission.Value);
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }

                EditorUtility.SetDirty(material);
                Materials[pair.Key] = material;
            }

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
                Debug.LogError($"[Scene3Bazaar] Missing TMP font: {FontPath}");

            AssetDatabase.SaveAssets();
        }

        private static void UpdateLevel()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            if (level == null)
            {
                Debug.LogError($"[Scene3Bazaar] Missing {LevelPath}");
                return;
            }

            level.displayName = "Level 3 - Bazaar Flamboyant";
            level.settingDescription = "Makan malam di bazar Flamboyant";
            level.learningFocus = new[] { "variasi", "pilihan seimbang" };

            var foods = new List<FoodData>();
            foreach (var id in StallFoodOrder)
            {
                var food = AssetDatabase.LoadAssetAtPath<FoodData>($"Assets/ScriptableObjects/Food/FD_{id}.asset");
                if (food != null)
                    foods.Add(food);
                else
                    Debug.LogError($"[Scene3Bazaar] Missing food asset for {id}.");
            }

            level.foodPool = foods.ToArray();
            level.minItems = 5;
            level.requiresDrink = false;
            EditorUtility.SetDirty(level);
        }

        private static void BuildScene()
        {
            EditorSceneManager.SaveOpenScenes();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            var rules = AssetDatabase.LoadAssetAtPath<NutritionRules>(RulesPath);
            if (level == null || rules == null || level.foodPool == null || level.foodPool.Length < 6)
            {
                Debug.LogError("[Scene3Bazaar] Missing level, rules, or eight-food pool.");
                return;
            }

            var root = new GameObject("Scene3Bazaar");
            CreateGround(root.transform);

            var school = InstantiateModel(SchoolFbx, root.transform, "SchoolCampus", new Vector3(20f, 0f, 0f), Quaternion.identity);
            if (school != null)
                AddBox(root.transform, "Collider_SchoolCampus", new Vector3(20f, 1.5f, 0f), new Vector3(36f, 3f, 60f));

            CreateWorldText(root.transform, "Sign_GateSchool", "SMA NEGERI 1 MANADO",
                new Vector3(1.55f, 3.95f, 0f), Quaternion.Euler(0f, 90f, 0f), 0.120f, 36f,
                new Color(0.85f, 0.66f, 0.22f), TextAlignmentOptions.Center);
            CreateWorldText(root.transform, "Sign_BuildingSchool", "SMA NEGERI 1 MANADO",
                new Vector3(23.9f, 7.05f, 0f), Quaternion.Euler(0f, 90f, 0f), 0.120f, 36f,
                new Color(0.85f, 0.66f, 0.22f), TextAlignmentOptions.Center);

            var cafeNorth = InstantiateModel(CafeFbx, root.transform, "Cafe_Kopi", new Vector3(-14.5f, 0f, 34f), Quaternion.Euler(0f, 90f, 0f));
            var cafeSouth = InstantiateModel(CafeFbx, root.transform, "Cafe_Bakar", new Vector3(-14.5f, 0f, -36f), Quaternion.Euler(0f, 90f, 0f));
            AddCafeColliders(cafeNorth);
            AddCafeColliders(cafeSouth);

            CreateWorldText(cafeNorth.transform, "Sign_CafeKopi", "KAFE KOPI",
                new Vector3(0f, 3.05f, 0.13f), Quaternion.Euler(0f, 180f, 0f), 0.120f, 36f,
                new Color(0.35f, 0.06f, 0.04f), TextAlignmentOptions.Center);
            CreateWorldText(cafeSouth.transform, "Sign_CafeBakar", "KAFE BAKAR",
                new Vector3(0f, 3.05f, 0.13f), Quaternion.Euler(0f, 180f, 0f), 0.120f, 36f,
                new Color(0.06f, 0.24f, 0.14f), TextAlignmentOptions.Center);
            CreateWorldText(root.transform, "Sign_CafeKopiNorth", "KAFE KOPI",
                new Vector3(-11.2f, 2.4f, 37.76f), Quaternion.Euler(0f, 180f, 0f), 0.080f, 36f,
                new Color(0.35f, 0.06f, 0.04f), TextAlignmentOptions.Center);

            CreateWorldText(root.transform, "Sign_StreetPramuka", "JL. PRAMUKA 19",
                new Vector3(-6f, 2.4f, -35.97f), Quaternion.Euler(0f, 180f, 0f), 0.028f, 36f,
                new Color(0.95f, 0.92f, 0.82f), TextAlignmentOptions.Center);

            InstantiateModel(StreetPropsFbx, root.transform, "StreetProps", Vector3.zero, Quaternion.identity);

            var stage = InstantiateModel(StageFbx, root.transform, "PanggungBazaar", new Vector3(StageX, 0f, StageZ), Quaternion.identity);
            if (stage != null)
            {
                AddBox(stage.transform, "Collider_Platform", new Vector3(0f, 0.15f, 0f), new Vector3(6f, 0.3f, 6f));
                AddBox(stage.transform, "Collider_Counter", new Vector3(0f, 0.40f, ServeZ), new Vector3(2.0f, 0.5f, 0.8f));
                CreateServingTable(stage.transform);
                CreateWorldText(stage.transform, "Sign_Panggung", "PANGGUNG BAZAAR",
                    new Vector3(0f, 2.55f, 3.17f), Quaternion.Euler(0f, 180f, 0f), 0.130f, 36f,
                    new Color(0.35f, 0.06f, 0.04f), TextAlignmentOptions.Center);
            }

            var stands = CreateFoodStalls(root.transform, level, rules);
            CreateCoffeeShops(root.transform);
            CreateSmkBuilding(root.transform);
            CreateNpcCast(root.transform);

            var platePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServingPlatePath);
            var plateObject = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab, root.transform);
            plateObject.name = "ServingPlate";
            plateObject.transform.localPosition = new Vector3(StageX - 0.35f, 0.652f, ServeZ);
            plateObject.transform.localRotation = Quaternion.identity;
            var plate = plateObject.GetComponent<PlateController>();

            var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServeStationPath);
            var stationObject = (GameObject)PrefabUtility.InstantiatePrefab(stationPrefab, root.transform);
            stationObject.name = "ServeStation";
            stationObject.transform.localPosition = new Vector3(StageX + 0.45f, 0.650f, ServeZ);
            stationObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var serveStation = stationObject.GetComponent<ServeStation>();

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRigPath);
            var playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerObject.name = "PlayerRig";
            playerObject.transform.position = new Vector3(-8.35f, 0.02f, 6.2f);
            playerObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var player = playerObject.GetComponent<FirstPersonController>();
            var reticle = playerObject.GetComponent<ReticleInteractor>();
            var carrier = playerObject.GetComponent<HeldFoodCarrier>();

            var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FigmaHudPath);
            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hud.name = "FigmaHUD";

            var flowObject = new GameObject("LevelFlow");
            var flow = flowObject.AddComponent<LevelFlowController>();

            flow.level = level;
            flow.rules = rules;
            flow.plate = plate;
            flow.stands = stands;
            flow.serveStation = serveStation;
            flow.player = player;
            flow.reticle = reticle;
            flow.carrier = carrier;
            flow.foodCards = hud.GetComponentInChildren<FoodCardGridPresenter>(true);
            flow.mealStatus = hud.GetComponentInChildren<MealStatusBarPresenter>(true);
            flow.prompt = hud.GetComponentInChildren<InteractionPrompt>(true);
            flow.infoCard = hud.GetComponentInChildren<FoodInfoCardPresenter>(true);
            flow.questSelect = hud.GetComponentInChildren<QuestSelectPresenter>(true);
            flow.campaignSummary = hud.GetComponentInChildren<CampaignSummaryPresenter>(true);
            flow.questOptions = QuestDataBuilder.LoadQuests();
            flow.finalResult = hud.GetComponentInChildren<FinalResultPresenter>(true);
            flow.pausePanel = FindDeepChild(hud.transform, "PausePanel")?.gameObject;
            flow.pauseMessage = FindDeepChild(hud.transform, "PauseMessage")?.GetComponent<TMP_Text>();
            flow.pauseResumeButton = FindDeepChild(hud.transform, "ResumeButton")?.GetComponent<Button>();
            flow.pauseQuitButton = FindDeepChild(hud.transform, "QuitButton")?.GetComponent<Button>();

            reticle.flow = flow;
            reticle.plate = plate;
            reticle.serveStation = serveStation;
            reticle.infoCard = flow.infoCard;
            reticle.foodCards = flow.foodCards;
            reticle.prompt = flow.prompt;

            ConfigureLighting(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateGround(Transform root)
        {
            GroundBox(root, "Ground_Base", new Vector3(13f, -0.05f, -2f), new Vector3(200f, 0.1f, 200f), "M_Bazaar_Grass", true);
            GroundBox(root, "Road_Flamboyant", new Vector3(-9f, 0.005f, -2f), new Vector3(6f, 0.01f, 88f), "M_Bazaar_Asphalt", false);
            GroundBox(root, "Sidewalk_West", new Vector3(-14f, 0.005f, -2f), new Vector3(4f, 0.01f, 88f), "M_Bazaar_Sidewalk", false);
            GroundBox(root, "Sidewalk_East", new Vector3(-4f, 0.005f, -2f), new Vector3(4f, 0.01f, 88f), "M_Bazaar_Sidewalk", false);
            GroundBox(root, "Plaza_Center", new Vector3(-9f, 0.007f, 0f), new Vector3(14f, 0.014f, 20f), "M_Bazaar_Plaza", false);
            GroundBox(root, "Forecourt_Gate", new Vector3(0f, 0.006f, 0f), new Vector3(4f, 0.012f, 12f), "M_Bazaar_Sidewalk", false);
            GroundBox(root, "Campus_Ground", new Vector3(20f, 0.005f, 0f), new Vector3(36f, 0.01f, 60f), "M_Bazaar_Concrete", false);
            GroundBox(root, "Pramuka_Road", new Vector3(13f, 0.005f, -43f), new Vector3(62f, 0.01f, 6f), "M_Bazaar_Asphalt", false);
            GroundBox(root, "Pramuka_Sidewalk", new Vector3(13f, 0.005f, -38.5f), new Vector3(62f, 0.01f, 3f), "M_Bazaar_Sidewalk", false);
            GroundBox(root, "Pramuka_Apron", new Vector3(25f, 0.005f, -33.5f), new Vector3(38f, 0.01f, 7f), "M_Bazaar_Concrete", false);
            GroundBox(root, "SMK_Ground", new Vector3(25f, 0.005f, -54f), new Vector3(38f, 0.01f, 12f), "M_Bazaar_Concrete", false);
            GroundBox(root, "Road_Flamboyant_Cross", new Vector3(-9f, 0.005f, 38f), new Vector3(120f, 0.01f, 6f), "M_Bazaar_Asphalt", false);
            GroundBox(root, "Sidewalk_Flamboyant_North", new Vector3(-9f, 0.005f, 41.5f), new Vector3(120f, 0.01f, 3f), "M_Bazaar_Sidewalk", false);
            GroundBox(root, "Sidewalk_Flamboyant_South", new Vector3(-9f, 0.005f, 34.5f), new Vector3(120f, 0.01f, 3f), "M_Bazaar_Sidewalk", false);
            GroundBox(root, "Intersection_Plaza", new Vector3(-9f, 0.007f, 38f), new Vector3(12f, 0.014f, 12f), "M_Bazaar_Plaza", false);

            // Curbs, kept visual so the player can cross the pedestrian street.
            GroundBox(root, "Curb_West", new Vector3(-12.1f, 0.06f, -2f), new Vector3(0.2f, 0.12f, 88f), "M_Bazaar_Curb", false);
            GroundBox(root, "Curb_East", new Vector3(-5.9f, 0.06f, -2f), new Vector3(0.2f, 0.12f, 88f), "M_Bazaar_Curb", false);
            GroundBox(root, "Curb_Pramuka", new Vector3(13f, 0.06f, -39.9f), new Vector3(62f, 0.12f, 0.2f), "M_Bazaar_Curb", false);
            GroundBox(root, "Curb_Flamboyant_North", new Vector3(-9f, 0.06f, 41.6f), new Vector3(120f, 0.12f, 0.2f), "M_Bazaar_Curb", false);
            GroundBox(root, "Curb_Flamboyant_South", new Vector3(-9f, 0.06f, 34.4f), new Vector3(120f, 0.12f, 0.2f), "M_Bazaar_Curb", false);

            // Crosswalks and a light dashed center line.
            for (int i = 0; i < 6; i++)
            {
                float x = -11.4f + i * 0.9f;
                GroundBox(root, $"Crosswalk_N_{i}", new Vector3(x, 0.012f, 8f), new Vector3(0.55f, 0.012f, 4f), "M_Bazaar_RoadLine", false);
                GroundBox(root, $"Crosswalk_S_{i}", new Vector3(x, 0.012f, -8f), new Vector3(0.55f, 0.012f, 4f), "M_Bazaar_RoadLine", false);
                GroundBox(root, $"Crosswalk_Flamboyant_{i}", new Vector3(x, 0.012f, 38f), new Vector3(0.55f, 0.012f, 4f), "M_Bazaar_RoadLine", false);
            }

            for (int i = 0; i < 15; i++)
            {
                float z = -34f + i * 5f;
                GroundBox(root, $"CenterLine_{i}", new Vector3(-9f, 0.012f, z), new Vector3(0.08f, 0.012f, 1.4f), "M_Bazaar_RoadLine", false);
            }

            for (int i = 0; i < 21; i++)
            {
                float x = -55f + i * 5f;
                GroundBox(root, $"CenterLine_Flamboyant_{i}", new Vector3(x, 0.012f, 38f), new Vector3(1.4f, 0.012f, 0.08f), "M_Bazaar_RoadLine", false);
            }

            // Invisible play-area boundary.
            AddBox(root, "Collider_Boundary_West", new Vector3(-58f, 1.5f, 0f), new Vector3(0.4f, 3f, 120f));
            AddBox(root, "Collider_Boundary_East", new Vector3(58f, 1.5f, 0f), new Vector3(0.4f, 3f, 120f));
            AddBox(root, "Collider_Boundary_South", new Vector3(0f, 1.5f, -58f), new Vector3(120f, 3f, 0.4f));
            AddBox(root, "Collider_Boundary_North", new Vector3(0f, 1.5f, 58f), new Vector3(120f, 3f, 0.4f));
        }

        private static void GroundBox(Transform parent, string name, Vector3 position, Vector3 size, string materialKey, bool collider)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;

            if (Materials.TryGetValue(materialKey, out var material))
                go.GetComponent<Renderer>().sharedMaterial = material;

            if (!collider)
                Object.DestroyImmediate(go.GetComponent<BoxCollider>());
        }

        private static void AddCafeColliders(GameObject cafe)
        {
            if (cafe == null)
                return;

            AddBox(cafe.transform, "Collider_Building", new Vector3(0f, 1.6f, -2f), new Vector3(7f, 3.2f, 4f));
            AddBox(cafe.transform, "Collider_Table_A", new Vector3(-1.6f, 0.4f, 2.2f), new Vector3(2.2f, 0.8f, 2.2f));
            AddBox(cafe.transform, "Collider_Table_B", new Vector3(1.6f, 0.4f, 2.2f), new Vector3(2.2f, 0.8f, 2.2f));
            AddBox(cafe.transform, "Collider_Menu", new Vector3(-3f, 0.8f, 1.4f), new Vector3(1f, 1.6f, 0.5f));
        }

        private static void CreateServingTable(Transform stage)
        {
            var table = new GameObject("ServingTable");
            table.transform.SetParent(stage, false);
            table.transform.localPosition = new Vector3(0f, 0f, ServeZ);

            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "ServingTableTop";
            top.transform.SetParent(table.transform, false);
            top.transform.localPosition = new Vector3(0f, 0.62f, 0f);
            top.transform.localScale = new Vector3(2.0f, 0.06f, 0.8f);
            top.GetComponent<Renderer>().sharedMaterial = Materials["M_Bazaar_WoodDark"];
            Object.DestroyImmediate(top.GetComponent<BoxCollider>());

            var cloth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name = "ServingTableCloth";
            cloth.transform.SetParent(table.transform, false);
            cloth.transform.localPosition = new Vector3(0f, 0.647f, 0f);
            cloth.transform.localScale = new Vector3(1.9f, 0.006f, 0.75f);
            cloth.GetComponent<Renderer>().sharedMaterial = Materials["M_Bazaar_CanvasCream"];
            Object.DestroyImmediate(cloth.GetComponent<BoxCollider>());

            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = "ServingTableLeg";
                    leg.transform.SetParent(table.transform, false);
                    leg.transform.localPosition = new Vector3(sx * 0.82f, 0.45f, sz * 0.30f);
                    leg.transform.localScale = new Vector3(0.08f, 0.34f, 0.08f);
                    leg.GetComponent<Renderer>().sharedMaterial = Materials["M_Bazaar_WoodDark"];
                    Object.DestroyImmediate(leg.GetComponent<BoxCollider>());
                }
            }
        }

        private static FoodStand[] CreateFoodStalls(Transform parent, LevelDefinition level, NutritionRules rules)
        {
            var byId = new Dictionary<string, FoodData>();
            foreach (var food in level.foodPool)
            {
                if (food != null)
                    byId[food.id] = food;
            }

            var placements = new List<StallPlacement>();
            AddStallRow(placements, -14.5f, 90f, new[] { 30f, 24f, 18f, 12f, 6f, -6f, -12f, -18f, -24f, -30f });
            AddStallRow(placements, -3.5f, -90f, new[] { 30f, 24f, 18f, 12f, 6f, -6f, -12f, -18f, -24f, -30f });
            AddStallCrossRow(placements, 34.5f, 0f, new[] { -40f, -30f, 8f });
            AddStallCrossRow(placements, 41.5f, 180f, new[] { -40f, -30f, 8f });
            AddStallCrossRow(placements, -35.5f, 180f, new[] { 8f, 16f, 24f, 32f });

            var stands = new List<FoodStand>();
            for (int i = 0; i < placements.Count; i++)
            {
                string firstId = StallFoodOrder[(i * 2) % StallFoodOrder.Length];
                string secondId = StallFoodOrder[(i * 2 + 1) % StallFoodOrder.Length];
                if (!byId.TryGetValue(firstId, out var first) || !byId.TryGetValue(secondId, out var second))
                {
                    Debug.LogError($"[Scene3Bazaar] Missing stall food {firstId}/{secondId}.");
                    continue;
                }

                var kind = FoodClassifier.Resolve(first, rules) == FoodHealthLabel.Healthy
                    && FoodClassifier.Resolve(second, rules) == FoodHealthLabel.Healthy
                    ? FoodStandKind.Healthy
                    : FoodStandKind.Junk;
                var displayName = first.displayName + " & " + second.displayName;
                var stand = CreateStand(parent, $"BazaarStand_{i + 1:00}", displayName, kind,
                    new[] { first, second }, placements[i].position, placements[i].yaw);
                if (stand != null)
                    stands.Add(stand);
            }

            return stands.ToArray();
        }

        private static void AddStallRow(List<StallPlacement> placements, float x, float yaw, float[] zValues)
        {
            foreach (var z in zValues)
                placements.Add(new StallPlacement(new Vector3(x, 0f, z), yaw));
        }

        private static void AddStallCrossRow(List<StallPlacement> placements, float z, float yaw, float[] xValues)
        {
            foreach (var x in xValues)
                placements.Add(new StallPlacement(new Vector3(x, 0f, z), yaw));
        }

        private static void CreateCoffeeShops(Transform root)
        {
            var shops = new[]
            {
                new { name = "CoffeeShop_01", position = new Vector3(-42f, 0f, 45f), yaw = 180f },
                new { name = "CoffeeShop_02", position = new Vector3(8f, 0f, 45f), yaw = 180f },
                new { name = "CoffeeShop_03", position = new Vector3(46f, 0f, 20f), yaw = 90f },
                new { name = "CoffeeShop_04", position = new Vector3(46f, 0f, -30f), yaw = 90f }
            };

            foreach (var shop in shops)
            {
                var cafe = InstantiateModel(CafeFbx, root, shop.name, shop.position, Quaternion.Euler(0f, shop.yaw, 0f));
                if (cafe == null)
                    continue;

                AddCafeColliders(cafe);
                CreateWorldText(cafe.transform, "Sign_Coffee", "COFFEE", new Vector3(0f, 3.05f, 0.13f),
                    Quaternion.Euler(0f, 180f, 0f), 0.110f, 36f,
                    new Color(0.35f, 0.06f, 0.04f), TextAlignmentOptions.Center);
            }
        }

        private static void CreateSmkBuilding(Transform root)
        {
            var smk = new GameObject("SMK_Building");
            smk.transform.SetParent(root, false);
            smk.transform.localPosition = new Vector3(30f, 0f, -58f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(smk.transform, false);
            body.transform.localPosition = new Vector3(0f, 3f, 0f);
            body.transform.localScale = new Vector3(30f, 6f, 10f);
            body.GetComponent<Renderer>().sharedMaterial = Materials["M_Bazaar_WallCream"];

            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "Roof";
            roof.transform.SetParent(smk.transform, false);
            roof.transform.localPosition = new Vector3(0f, 6.2f, 0f);
            roof.transform.localScale = new Vector3(31f, 0.4f, 11f);
            roof.GetComponent<Renderer>().sharedMaterial = Materials["M_Bazaar_RoofDark"];
            Object.DestroyImmediate(roof.GetComponent<BoxCollider>());

            CreateWorldText(root, "Sign_SMK", "SMK NEGERI 1 MANADO",
                new Vector3(30f, 6.7f, -52.7f), Quaternion.identity, 0.085f, 36f,
                new Color(0.85f, 0.66f, 0.22f), TextAlignmentOptions.Center);
        }

        private static FoodStand CreateStand(Transform parent, string standName, string displayName, FoodStandKind kind, FoodData[] foods, Vector3 position, float yaw)
        {
            var station = new GameObject(standName);
            station.transform.SetParent(parent, false);
            station.transform.localPosition = position;
            station.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var model = InstantiateModel(StandFbx, station.transform, "Model", Vector3.zero, Quaternion.identity);
            if (model == null)
                return null;

            var stand = station.AddComponent<FoodStand>();
            stand.displayName = displayName;
            stand.kind = kind;
            stand.foods = foods;
            stand.anchors = new Transform[foods.Length];
            stand.enabled = false;

            var foodRoot = new GameObject($"{standName}_Food");
            foodRoot.transform.SetParent(parent, false);

            for (int i = 0; i < foods.Length; i++)
            {
                var food = foods[i];
                if (food == null)
                    continue;

                float x = StandItemX(foods.Length, i);
                float z = StandItemZ(foods.Length, i);
                var anchor = new GameObject($"Anchor_{food.id}");
                anchor.transform.SetParent(station.transform, false);
                anchor.transform.localPosition = new Vector3(x, 0.98f, z);
                anchor.transform.localRotation = Quaternion.identity;
                stand.anchors[i] = anchor.transform;

                var dishPath = IsBowl(food.id) ? BowlFbx : PlateFbx;
                var dish = InstantiateModel(dishPath, station.transform, $"Dish_{food.id}",
                    new Vector3(x, 0.95f, z), Quaternion.identity);
                if (dish != null)
                    dish.transform.localScale = Vector3.one * 0.7f;

                if (food.prefab == null)
                {
                    Debug.LogError($"[Scene3Bazaar] Missing food prefab for {food.id}.");
                    continue;
                }

                const int portionCount = 2;
                for (int portion = 0; portion < portionCount; portion++)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(food.prefab, foodRoot.transform);
                    instance.name = portion == 0 ? food.id : $"{food.id}_{portion + 1}";
                    var spawnPosition = anchor.transform.position + anchor.transform.rotation * PortionOffset(portion, portionCount);
                    instance.transform.position = spawnPosition;
                    instance.transform.rotation = anchor.transform.rotation;
                    var item = instance.GetComponent<FoodItem>();
                    item.data = food;
                    item.SetHome(stand, spawnPosition, anchor.transform.rotation);
                    instance.transform.SetParent(foodRoot.transform, true);
                }
            }

            var signKey = displayName.Replace(" ", string.Empty);
            var sign = CreateWorldText(station.transform, $"Sign_{signKey}", displayName,
                new Vector3(0f, 2.9f, 1.29f), Quaternion.Euler(0f, 180f, 0f), 0.060f, 36f,
                new Color(0.14f, 0.10f, 0.07f), TextAlignmentOptions.Center);
            stand.signText = sign;
            CreateWorldText(station.transform, $"SignFront_{signKey}", displayName,
                new Vector3(0f, 0.55f, 1.28f), Quaternion.Euler(0f, 180f, 0f), 0.070f, 36f,
                new Color(0.14f, 0.10f, 0.07f), TextAlignmentOptions.Center);

            AddBox(station.transform, "Collider_Counter", new Vector3(0f, 0.45f, 0.85f), new Vector3(2.9f, 0.9f, 0.8f));
            AddBox(station.transform, "Collider_Back", new Vector3(0f, 1.3f, -0.85f), new Vector3(2.9f, 1f, 0.5f));

            CreatePromoBoard(station.transform, kind);
            return stand;
        }

        private static float StandItemX(int count, int index)
        {
            if (count >= 5)
                return index < 3 ? (index - 1) * 0.42f : (index - 3 - 0.5f) * 0.42f;

            return (index - (count - 1) * 0.5f) * 0.45f;
        }

        private static float StandItemZ(int count, int index)
        {
            // The stand model has decorative produce crates at both ends of the counter,
            // so the usable area is the middle. Five foods use a 3 + 2 back/front layout.
            if (count < 5)
                return 0.85f;

            return index >= 3 ? 1.07f : 0.63f;
        }

        private static int PortionCount(string foodId)
        {
            return foodId switch
            {
                "pisang" => 4,
                "tempe" => 3,
                "brokoli" => 3,
                "ayam_goreng" => 3,
                _ => 1
            };
        }

        private static Vector3 PortionOffset(int index, int count)
        {
            if (count <= 1)
                return Vector3.zero;

            if (count == 3)
            {
                return index switch
                {
                    0 => new Vector3(-0.045f, 0f, -0.025f),
                    1 => new Vector3(0.045f, 0f, 0.025f),
                    _ => new Vector3(0f, 0.015f, 0f)
                };
            }

            return index switch
            {
                0 => new Vector3(-0.05f, 0f, -0.03f),
                1 => new Vector3(0.05f, 0f, -0.03f),
                2 => new Vector3(-0.05f, 0.01f, 0.03f),
                _ => new Vector3(0.05f, 0.01f, 0.03f)
            };
        }

        private static void CreatePromoBoard(Transform standTransform, FoodStandKind kind)
        {
            var boardRoot = new GameObject("PromoBoard");
            boardRoot.transform.SetParent(standTransform, false);
            boardRoot.transform.localPosition = new Vector3(1.75f, 0f, 0.9f);
            boardRoot.transform.localRotation = Quaternion.identity;

            CreatePrimitivePart(PrimitiveType.Cube, boardRoot.transform, "Post",
                new Vector3(0f, 0.6f, 0f), new Vector3(0.07f, 1.2f, 0.07f), "M_Bazaar_WoodDark");
            string boardMaterial = kind == FoodStandKind.Healthy ? "M_Bazaar_CanvasCream" : "M_Bazaar_Chalkboard";
            CreatePrimitivePart(PrimitiveType.Cube, boardRoot.transform, "Board",
                new Vector3(0f, 1.25f, 0f), new Vector3(1.0f, 0.6f, 0.05f), boardMaterial);

            Color textColor = kind == FoodStandKind.Healthy
                ? new Color(0.14f, 0.10f, 0.07f)
                : new Color(0.95f, 0.90f, 0.75f);
            string label = kind == FoodStandKind.Healthy ? "SAYUR SEGAR\nHARI INI" : "JAJANAN\nPROMO!";
            var text = CreateWorldText(boardRoot.transform, "Text", label,
                new Vector3(0f, 1.25f, 0.04f), Quaternion.Euler(0f, 180f, 0f), 0.030f, 26f,
                textColor, TextAlignmentOptions.Center);
            text.textWrappingMode = TextWrappingModes.Normal;
            text.rectTransform.sizeDelta = new Vector2(34f, 20f);
        }

        private static GameObject CreatePrimitivePart(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            string materialKey)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            if (Materials.TryGetValue(materialKey, out var material) && material != null)
                go.GetComponent<Renderer>().sharedMaterial = material;

            return go;
        }

        private static void CreateDecorStalls(Transform parent)
        {
            CreateDecorStall(parent, "DecorStall_Flamboyant_01", new Vector3(-3.5f, 0f, 33f), -90f);
            CreateDecorStall(parent, "DecorStall_Flamboyant_02", new Vector3(-3.5f, 0f, -33f), -90f);
            CreateDecorStall(parent, "DecorStall_Pramuka_01", new Vector3(8f, 0f, -35.5f), 180f);
            CreateDecorStall(parent, "DecorStall_Pramuka_02", new Vector3(16f, 0f, -35.5f), 180f);

            for (int i = 0; i < ClosedStandPositions.Length; i++)
                CreateDecorStall(parent, $"DecorStall_Closed_{i + 1:00}", ClosedStandPositions[i], ClosedStandYaws[i]);
        }

        private static void CreateDecorStall(Transform parent, string name, Vector3 position, float yaw)
        {
            var stall = new GameObject(name);
            stall.transform.SetParent(parent, false);
            stall.transform.localPosition = position;
            stall.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            InstantiateModel(ClosedStandFbx, stall.transform, "Model", Vector3.zero, Quaternion.identity);

            var decor = stall.AddComponent<DecorStall>();
            decor.displayName = name;

            CreateWorldText(stall.transform, "Sign_Tutup", "TUTUP",
                new Vector3(0f, 1.3f, 1.29f), Quaternion.Euler(0f, 180f, 0f), 0.060f, 36f,
                new Color(0.28f, 0.28f, 0.28f), TextAlignmentOptions.Center);

            AddBox(stall.transform, "Collider_Stall", new Vector3(0f, 0.5f, 0.85f), new Vector3(2.9f, 1f, 0.8f));
        }

        private static void CreateNpcCast(Transform parent)
        {
            var npcRoot = new GameObject("NPCs");
            npcRoot.transform.SetParent(parent, false);

            PlaceNpc(npcRoot.transform, "NPC_Male_B", "Npc_Walker_Male", new Vector3(-10.8f, 0.02f, 36f), 180f,
                "Visitor", "Walk", 0f, new[]
                {
                    new Vector3(-10.8f, 0.02f, 36f),
                    new Vector3(-10.8f, 0.02f, 12f),
                    new Vector3(-10.8f, 0.02f, 6f)
                });
            PlaceNpc(npcRoot.transform, "NPC_Female_B", "Npc_Walker_Female", new Vector3(-7.0f, 0.02f, 38f), 0f,
                "Visitor", "Walk", 0.2f, new[]
                {
                    new Vector3(-7.0f, 0.02f, 38f),
                    new Vector3(-7.0f, 0.02f, 16f),
                    new Vector3(-7.0f, 0.02f, 8f)
                });

            PlaceNpc(npcRoot.transform, "NPC_Male_A", "Npc_Stand_West_01", new Vector3(-12.6f, 0.02f, 28f), -90f, "Visitor", "Idle", 0.15f, null);
            PlaceNpc(npcRoot.transform, "NPC_Female_A", "Npc_Stand_West_03", new Vector3(-12.6f, 0.02f, 18f), -90f, "Visitor", "Wipe", 0.4f, null);
            PlaceNpc(npcRoot.transform, "NPC_FemaleHijab_A", "Npc_Stand_East_06", new Vector3(-5.4f, 0.02f, -13f), 90f, "Visitor", "Idle", 0.6f, null);
            PlaceNpc(npcRoot.transform, "NPC_Male_A", "Npc_Stand_East_07", new Vector3(-5.4f, 0.02f, -23f), 90f, "Visitor", "Talk", 0.3f, null);
            PlaceNpc(npcRoot.transform, "NPC_FemaleHijab_B", "Npc_Cafe_Guest", new Vector3(-12.9f, 0.02f, 34.0f), 90f, "CafeGuest", "Talk", 0.55f, null);
            PlaceNpc(npcRoot.transform, "NPC_Male_B", "Npc_Stage_Audience_01", new Vector3(-9.3f, 0.02f, 5.4f), 180f, "Audience", "Idle", 0.25f, null);
            PlaceNpc(npcRoot.transform, "NPC_Female_A", "Npc_Stage_Audience_02", new Vector3(-6.3f, 0.02f, 5.4f), 180f, "Audience", "Talk", 0.7f, null);
        }

        private static GameObject PlaceNpc(
            Transform parent, string prefabName, string name, Vector3 position, float yaw,
            string role, string clip, float phase, Vector3[] waypoints)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{NpcFolder}/{prefabName}.prefab");
            if (prefab == null)
            {
                Debug.LogError($"[Scene3Bazaar] Missing NPC prefab {prefabName}.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            NpcKitBuilder.ConfigureRole(instance, false, false);

            var marker = instance.GetComponent<NpcMarker>();
            if (marker != null)
            {
                marker.role = role;
                marker.seated = false;
            }

            var animator = instance.GetComponent<NpcAnimator>();
            if (animator != null)
            {
                animator.defaultState = clip;
                animator.phaseOffset = phase;
            }

            var walker = instance.GetComponent<NpcWalker>();
            if (walker != null && waypoints != null && waypoints.Length > 1)
            {
                walker.waypoints = waypoints;
                walker.speed = 0.9f;
                walker.pauseSeconds = 1.3f;
            }

            return instance;
        }

        private static bool IsBowl(string foodId)
        {
            return foodId == "rendang" || foodId == "brokoli" || foodId == "mie_goreng";
        }

        private static GameObject InstantiateModel(string path, Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogError($"[Scene3Bazaar] Missing model: {path}");
                return null;
            }

            var wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);
            wrapper.transform.localPosition = localPosition;
            wrapper.transform.localRotation = localRotation;
            wrapper.transform.localScale = Vector3.one;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, wrapper.transform);
            instance.name = "Model";
            // Keep the FBX import rotation (270 degrees around X) and scale intact.
            RemapMaterials(instance);
            return wrapper;
        }

        private static string NormalizeMaterialName(string name)
        {
            return name.Replace(" (Instance)", string.Empty).Split('.')[0].Trim();
        }

        private static void RemapMaterials(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int index = 0; index < materials.Length; index++)
                {
                    if (materials[index] == null)
                        continue;

                    var key = NormalizeMaterialName(materials[index].name);
                    if (Materials.TryGetValue(key, out var replacement))
                    {
                        materials[index] = replacement;
                    }
                    else if (MissingMaterials.Add(key))
                    {
                        Debug.LogWarning($"[Scene3Bazaar] No URP material for '{key}' on {renderer.name}.");
                    }
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static TextMeshPro CreateWorldText(
            Transform parent,
            string name,
            string text,
            Vector3 localPosition,
            Quaternion localRotation,
            float scale,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = Vector3.one * scale;

            var tmp = go.AddComponent<TextMeshPro>();
            if (font != null)
            {
                tmp.font = font;
                tmp.fontSharedMaterial = font.material;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.rectTransform.sizeDelta = new Vector2(20f, 4f);
            tmp.raycastTarget = false;
            return tmp;
        }

        private static void ConfigureLighting(Transform root)
        {
            var sunObject = new GameObject("Sun");
            sunObject.transform.SetParent(root, false);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(35f, -35f, 0f);
            sun.color = new Color(1f, 0.88f, 0.72f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            RenderSettings.sun = sun;

            var fillRoot = new GameObject("Lighting");
            fillRoot.transform.SetParent(root, false);
            AddPointLight(fillRoot.transform, "Fill_Stage", new Vector3(StageX, 2.6f, StageZ), 7f, 0.55f);
            AddPointLight(fillRoot.transform, "Fill_CafeNorth", new Vector3(-13f, 2.6f, 36f), 6f, 0.4f);
            AddPointLight(fillRoot.transform, "Fill_CafeSouth", new Vector3(-13f, 2.6f, -35f), 6f, 0.4f);
            AddPointLight(fillRoot.transform, "Fill_FlamboyantCross", new Vector3(StageX, 2.6f, 38f), 12f, 0.55f);
            AddPointLight(fillRoot.transform, "Fill_BazaarEast", new Vector3(18f, 2.6f, 38f), 10f, 0.4f);
            AddPointLight(fillRoot.transform, "Fill_CoffeeShops", new Vector3(-30f, 2.6f, 42f), 10f, 0.35f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.47f, 0.42f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.64f, 0.55f, 0.46f);
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 140f;

            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var skyPath = "Assets/Materials/Bazaar/M_Bazaar_Sky.mat";
                var sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
                if (sky == null)
                {
                    sky = new Material(skyShader);
                    AssetDatabase.CreateAsset(sky, skyPath);
                }

                sky.shader = skyShader;
                sky.SetColor("_SkyTint", new Color(0.58f, 0.55f, 0.50f));
                sky.SetColor("_GroundColor", new Color(0.30f, 0.26f, 0.22f));
                sky.SetFloat("_AtmosphereThickness", 1.1f);
                sky.SetFloat("_SunSize", 0.04f);
                sky.SetFloat("_Exposure", 1.1f);
                EditorUtility.SetDirty(sky);
                RenderSettings.skybox = sky;
            }

            DynamicGI.UpdateEnvironment();
        }

        private static void AddPointLight(Transform parent, string name, Vector3 position, float range, float intensity)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = new Color(1f, 0.78f, 0.52f);
            light.shadows = LightShadows.None;
        }

        private static BoxCollider AddBox(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var colliderObject = new GameObject(name);
            colliderObject.transform.SetParent(parent, false);
            var collider = colliderObject.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
            return collider;
        }

        private static void UpdateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => File.Exists(scene.path))
                .ToList();

            if (scenes.All(scene => scene.path != ScenePath))
            {
                var mainIndex = scenes.FindIndex(scene => scene.path == "Assets/Scenes/MainVR.unity");
                scenes.Insert(mainIndex >= 0 ? mainIndex + 1 : scenes.Count, new EditorBuildSettingsScene(ScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
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
