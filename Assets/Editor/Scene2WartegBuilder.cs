using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    public static class Scene2WartegBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level02_Warteg.unity";
        private const string LegacyLevelPath = "Assets/ScriptableObjects/Levels/LD_Level02_Padang.asset";
        private const string LevelPath = "Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset";
        private const string RulesPath = "Assets/ScriptableObjects/Rules/NutritionRules.asset";
        private const string PlayerRigPath = "Assets/Prefabs/Player/PlayerRig.prefab";
        private const string FigmaHudPath = "Assets/Prefabs/UI/FigmaHUD.prefab";
        private const string ServingPlatePath = "Assets/Prefabs/Interaction/ServingPlate.prefab";
        private const string ServeStationPath = "Assets/Prefabs/Interaction/ServeStation.prefab";
        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string NpcFolder = "Assets/Prefabs/NPC";

        private const string ShellFbx = "Assets/Models/Environment/Warteg/WartegShell.fbx";
        private const string CounterFbx = "Assets/Models/Environment/Warteg/WartegCounter.fbx";
        private const string FurnitureFbx = "Assets/Models/Environment/Warteg/WartegFurniture.fbx";
        private const string PropsFbx = "Assets/Models/Environment/Warteg/WartegProps.fbx";
        private const string PlateFbx = "Assets/Models/Food/Plating/Plate_Small.fbx";
        private const string BowlFbx = "Assets/Models/Food/Plating/Bowl_Small.fbx";

        private const float FloorY = 0.16f;
        private const float CounterZ = -1.4f;
        private const float TrayY = 1.05f;

        private static readonly string[] FoodIds =
        {
            "nasi_putih", "rendang", "ayam_goreng", "telur_ceplok", "tempe", "sayur_singkong", "kerupuk"
        };

        private class MatSpec
        {
            public Color color;
            public float smoothness;
            public float metallic;
            public Color? emission;
            public bool transparent;
        }

        private static readonly Dictionary<string, MatSpec> Palette = new()
        {
            { "M_Warteg_WallCream", new MatSpec { color = new Color(0.90f, 0.85f, 0.72f), smoothness = 0.10f } },
            { "M_Warteg_WallWarm", new MatSpec { color = new Color(0.80f, 0.66f, 0.48f), smoothness = 0.10f } },
            { "M_Warteg_WallLight", new MatSpec { color = new Color(0.96f, 0.92f, 0.82f), smoothness = 0.10f } },
            { "M_Warteg_Wood", new MatSpec { color = new Color(0.52f, 0.34f, 0.19f), smoothness = 0.18f } },
            { "M_Warteg_WoodDark", new MatSpec { color = new Color(0.30f, 0.19f, 0.10f), smoothness = 0.18f } },
            { "M_Warteg_Terracotta", new MatSpec { color = new Color(0.66f, 0.30f, 0.18f), smoothness = 0.14f } },
            { "M_Warteg_Tile", new MatSpec { color = new Color(0.76f, 0.70f, 0.60f), smoothness = 0.18f } },
            { "M_Warteg_TileDark", new MatSpec { color = new Color(0.32f, 0.30f, 0.28f), smoothness = 0.18f } },
            { "M_Warteg_AwningRed", new MatSpec { color = new Color(0.62f, 0.12f, 0.10f), smoothness = 0.06f } },
            { "M_Warteg_AwningCream", new MatSpec { color = new Color(0.94f, 0.88f, 0.74f), smoothness = 0.06f } },
            { "M_Warteg_Metal", new MatSpec { color = new Color(0.55f, 0.56f, 0.54f), metallic = 1f, smoothness = 0.65f } },
            { "M_Warteg_MetalDark", new MatSpec { color = new Color(0.07f, 0.07f, 0.07f), metallic = 0.7f, smoothness = 0.45f } },
            { "M_Warteg_Glass", new MatSpec { color = new Color(0.55f, 0.72f, 0.70f, 0.32f), smoothness = 0.95f, transparent = true } },
            { "M_Warteg_Bulb", new MatSpec { color = new Color(1f, 0.80f, 0.45f), smoothness = 0.3f, emission = new Color(1f, 0.62f, 0.25f) * 2.5f } },
            { "M_Warteg_Leaf", new MatSpec { color = new Color(0.16f, 0.40f, 0.16f), smoothness = 0.08f } },
            { "M_Warteg_LeafLight", new MatSpec { color = new Color(0.26f, 0.52f, 0.20f), smoothness = 0.08f } },
            { "M_Warteg_Gas", new MatSpec { color = new Color(0.82f, 0.38f, 0.10f), smoothness = 0.3f } },
            { "M_Warteg_CrateRed", new MatSpec { color = new Color(0.65f, 0.12f, 0.10f), smoothness = 0.15f } },
            { "M_Warteg_CrateBlue", new MatSpec { color = new Color(0.10f, 0.24f, 0.45f), smoothness = 0.15f } },
            { "M_Warteg_Chalkboard", new MatSpec { color = new Color(0.07f, 0.16f, 0.12f), smoothness = 0.08f } },
            { "M_Warteg_SignGreen", new MatSpec { color = new Color(0.06f, 0.22f, 0.13f), smoothness = 0.15f } },
            { "M_Warteg_SignGold", new MatSpec { color = new Color(0.84f, 0.64f, 0.22f), metallic = 0.3f, smoothness = 0.45f } },
            { "M_Warteg_Ceramic", new MatSpec { color = new Color(0.94f, 0.92f, 0.86f), smoothness = 0.32f } },
            { "M_Warteg_Rice", new MatSpec { color = new Color(0.93f, 0.90f, 0.82f), smoothness = 0.15f } },
            { "M_Warteg_Concrete", new MatSpec { color = new Color(0.62f, 0.60f, 0.54f), smoothness = 0.12f } },
        };

        private static readonly Dictionary<string, Material> Materials = new();
        private static readonly HashSet<string> MissingMaterials = new();
        private static TMP_FontAsset font;
        private static LevelDefinition level;

        [MenuItem("NutriPick/Build Scene 2 Warteg")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            ConfigureModelImporters();
            EnsureMaterials();
            EnsureLevel();

            if (level == null || level.foodPool == null || level.foodPool.Length != 7)
            {
                Debug.LogError("[Scene2Warteg] Level 2 needs the seven-food Warteg pool.");
                return;
            }

            BuildScene();
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            NutriPickSceneValidator.ValidateScenes();
            NutriPickSceneValidator.CaptureScenePreviews();

            Debug.Log("[Scene2Warteg] Built open-front Warteg Tegal Sari with seven dishes, seating, and five NPCs.");
        }

        private static void ConfigureModelImporters()
        {
            var paths = new[] { ShellFbx, CounterFbx, FurnitureFbx, PropsFbx };

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"[Scene2Warteg] Missing FBX: {path}");
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
                importer.generateSecondaryUV = false;
                importer.meshCompression = ModelImporterMeshCompression.Off;
                importer.isReadable = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureMaterials()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials/Warteg"))
                AssetDatabase.CreateFolder("Assets/Materials", "Warteg");

            var litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                Debug.LogError("[Scene2Warteg] URP Lit shader not found.");
                return;
            }

            foreach (var pair in Palette)
            {
                var path = $"Assets/Materials/Warteg/{pair.Key}.mat";
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

                if (pair.Value.transparent)
                {
                    material.SetFloat("_Surface", 1f);
                    material.SetFloat("_Blend", 0f);
                    material.SetFloat("_AlphaClip", 0f);
                    material.SetFloat("_ZWrite", 0f);
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.renderQueue = (int)RenderQueue.Transparent;
                    material.SetShaderPassEnabled("ShadowCaster", false);
                }

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
                Debug.LogError($"[Scene2Warteg] Missing TMP font: {FontPath}");

            AssetDatabase.SaveAssets();
        }

        private static void EnsureLevel()
        {
            if (!File.Exists(LevelPath) && File.Exists(LegacyLevelPath))
                AssetDatabase.MoveAsset(LegacyLevelPath, LevelPath);

            level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(level, LevelPath);
            }

            var foods = new List<FoodData>();
            foreach (var id in FoodIds)
            {
                var food = AssetDatabase.LoadAssetAtPath<FoodData>($"Assets/ScriptableObjects/Food/FD_{id}.asset");
                if (food == null)
                {
                    Debug.LogError($"[Scene2Warteg] Missing food asset for {id}.");
                    continue;
                }

                foods.Add(food);
            }

            level.levelId = "level_02_warteg";
            level.displayName = "Level 2 — Warteg";
            level.sceneName = "Level02_Warteg";
            level.nextSceneName = "Level03_Bazaar";
            level.levelIndex = 2;
            level.settingDescription = "Makan siang di Warteg Tegal Sari";
            level.foodPool = foods.ToArray();
            level.minItems = 4;
            level.requiresDrink = false;
            level.minimumStands = 1;
            level.isFinalLevel = false;
            level.learningFocus = new[] { "porsi", "lemak", "natrium" };
            EditorUtility.SetDirty(level);

            var main = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_MainVR.asset");
            if (main != null)
            {
                main.nextSceneName = "Level02_Warteg";
                EditorUtility.SetDirty(main);
            }

            AssetDatabase.SaveAssets();
        }

        private static void BuildScene()
        {
            EditorSceneManager.SaveOpenScenes();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var rules = AssetDatabase.LoadAssetAtPath<NutritionRules>(RulesPath);
            if (level == null || rules == null)
            {
                Debug.LogError("[Scene2Warteg] Missing level or rules.");
                return;
            }

            var root = new GameObject("Scene2Warteg");
            CreateGround(root.transform);
            CreateNeighbours(root.transform);
            CreateStreetTrees(root.transform);

            var warteg = new GameObject("WartegTegalSari");
            warteg.transform.SetParent(root.transform, false);

            InstantiateModel(ShellFbx, warteg.transform, "Shell", Vector3.zero, Quaternion.identity);
            InstantiateModel(CounterFbx, warteg.transform, "Counter", Vector3.zero, Quaternion.identity);
            InstantiateModel(FurnitureFbx, warteg.transform, "Furniture", Vector3.zero, Quaternion.identity);
            InstantiateModel(PropsFbx, warteg.transform, "Props", Vector3.zero, Quaternion.identity);

            HideNamed(warteg.transform, "TrayFood");
            HideNamed(warteg.transform, "SignBanner_Text");

            AddWartegColliders(warteg.transform);
            CreateSignage(warteg.transform);

            var display = CreateFoodDisplay(warteg.transform);
            var plateObject = CreateServingStation(warteg.transform);
            var plateController = plateObject.GetComponent<PlateController>();
            var serveStation = FindDeepChild(warteg.transform, "ServeStation").GetComponent<ServeStation>();

            CreateNpcCast(warteg.transform);

            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRigPath);
            var playerObject = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerObject.name = "PlayerRig";
            playerObject.transform.position = new Vector3(0f, 0.14f, 8.2f);
            playerObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            var player = playerObject.GetComponent<FirstPersonController>();
            var reticle = playerObject.GetComponent<ReticleInteractor>();

            var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FigmaHudPath);
            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hud.name = "FigmaHUD";

            var flowObject = new GameObject("LevelFlow");
            var flow = flowObject.AddComponent<LevelFlowController>();

            flow.level = level;
            flow.rules = rules;
            flow.plate = plateController;
            flow.stands = new[] { display };
            flow.serveStation = serveStation;
            flow.player = player;
            flow.input = playerObject.GetComponent<GamepadInput>();
            flow.reticle = reticle;
            flow.foodCards = hud.GetComponentInChildren<FoodCardGridPresenter>(true);
            flow.mealStatus = hud.GetComponentInChildren<MealStatusBarPresenter>(true);
            flow.prompt = hud.GetComponentInChildren<InteractionPrompt>(true);
            flow.infoCard = hud.GetComponentInChildren<FoodInfoCardPresenter>(true);
            flow.report = hud.GetComponentInChildren<HealthReportPresenter>(true);
            flow.finalResult = hud.GetComponentInChildren<FinalResultPresenter>(true);
            flow.pausePanel = FindDeepChild(hud.transform, "PausePanel")?.gameObject;
            flow.pauseMessage = FindDeepChild(hud.transform, "PauseMessage")?.GetComponent<TMP_Text>();
            flow.pauseResumeButton = FindDeepChild(hud.transform, "ResumeButton")?.GetComponent<Button>();
            flow.pauseQuitButton = FindDeepChild(hud.transform, "QuitButton")?.GetComponent<Button>();

            reticle.flow = flow;
            reticle.plate = plateController;
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
            GroundBox(root, "Ground_Grass", new Vector3(0f, -0.07f, 5f), new Vector3(90f, 0.1f, 90f), "M_Warteg_Leaf", true);
            GroundBox(root, "Sidewalk", new Vector3(0f, 0.06f, 6.25f), new Vector3(60f, 0.12f, 6.5f), "M_Warteg_Tile", true);
            GroundBox(root, "Road_Asphalt", new Vector3(0f, 0.005f, 13.2f), new Vector3(60f, 0.01f, 7.4f), "M_Warteg_TileDark", false);
            GroundBox(root, "Curb", new Vector3(0f, 0.07f, 9.5f), new Vector3(60f, 0.14f, 0.16f), "M_Warteg_Concrete", true);

            for (int i = 0; i < 12; i++)
                GroundBox(root, $"RoadLine_{i}", new Vector3(-27f + i * 5f, 0.012f, 13.2f), new Vector3(2.2f, 0.012f, 0.10f), "M_Warteg_AwningCream", false);

            AddBox(root, "Collider_Boundary_West", new Vector3(-11f, 1.5f, 5f), new Vector3(0.4f, 3f, 24f));
            AddBox(root, "Collider_Boundary_East", new Vector3(11f, 1.5f, 5f), new Vector3(0.4f, 3f, 24f));
            AddBox(root, "Collider_Boundary_South", new Vector3(0f, 1.5f, -5.5f), new Vector3(22f, 3f, 0.4f));
            AddBox(root, "Collider_Boundary_North", new Vector3(0f, 1.5f, 17f), new Vector3(22f, 3f, 0.4f));
        }

        private static void CreateNeighbours(Transform root)
        {
            CreateShophouse(root, "Ruko_West_1", new Vector3(-9.8f, 0f, 1.4f), 4.4f, 5.2f, 3.4f, 0f, true, 0);
            CreateShophouse(root, "Ruko_West_2", new Vector3(-15.0f, 0f, 1.4f), 4.4f, 5.2f, 4.0f, 0f, false, 1);
            CreateShophouse(root, "Ruko_East_1", new Vector3(9.8f, 0f, 1.4f), 4.4f, 5.2f, 3.6f, 180f, true, 2);
            CreateShophouse(root, "Ruko_East_2", new Vector3(15.0f, 0f, 1.4f), 4.4f, 5.2f, 4.2f, 180f, false, 3);
            CreateShophouse(root, "Building_North_1", new Vector3(-7f, 0f, 21f), 6f, 5f, 5.2f, 180f, false, 4);
            CreateShophouse(root, "Building_North_2", new Vector3(7f, 0f, 21f), 6f, 5f, 4.6f, 180f, false, 5);

            CreateStreetFence(root);
            CreateStreetLamp(root, new Vector3(-10.6f, 0f, 9.8f));
            CreateStreetLamp(root, new Vector3(10.6f, 0f, 9.8f));
        }

        private static void CreateShophouse(
            Transform root,
            string name,
            Vector3 position,
            float width,
            float depth,
            float height,
            float yaw,
            bool collide,
            int variant)
        {
            var wallMaterials = new[]
            {
                "M_Warteg_WallWarm", "M_Warteg_WallCream", "M_Warteg_Terracotta",
                "M_Warteg_WallLight", "M_Warteg_WallWarm", "M_Warteg_WallCream"
            };
            var roofMaterials = new[]
            {
                "M_Warteg_MetalDark", "M_Warteg_CrateRed", "M_Warteg_Terracotta",
                "M_Warteg_MetalDark", "M_Warteg_CrateRed", "M_Warteg_MetalDark"
            };
            var awningMaterials = new[]
            {
                "M_Warteg_AwningRed", "M_Warteg_AwningCream", "M_Warteg_SignGreen",
                "M_Warteg_AwningRed", "M_Warteg_AwningCream", "M_Warteg_CrateBlue"
            };

            var building = new GameObject(name);
            building.transform.SetParent(root, false);
            building.transform.localPosition = position;
            building.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = name + "_Body";
            body.transform.SetParent(building.transform, false);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(width, height, depth);
            body.GetComponent<Renderer>().sharedMaterial = Materials[wallMaterials[variant % wallMaterials.Length]];
            if (!collide)
                Object.DestroyImmediate(body.GetComponent<BoxCollider>());

            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = name + "_Roof";
            roof.transform.SetParent(building.transform, false);
            roof.transform.localPosition = new Vector3(0f, height + 0.15f, 0f);
            roof.transform.localScale = new Vector3(width + 0.6f, 0.3f, depth + 0.8f);
            roof.GetComponent<Renderer>().sharedMaterial = Materials[roofMaterials[variant % roofMaterials.Length]];
            Object.DestroyImmediate(roof.GetComponent<BoxCollider>());

            var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = name + "_FloorBand";
            band.transform.SetParent(building.transform, false);
            band.transform.localPosition = new Vector3(0f, height * 0.66f, 0f);
            band.transform.localScale = new Vector3(width + 0.12f, 0.12f, depth + 0.12f);
            band.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_WoodDark"];
            Object.DestroyImmediate(band.GetComponent<BoxCollider>());

            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = name + "_Door";
            door.transform.SetParent(building.transform, false);
            door.transform.localPosition = new Vector3(0f, 0.95f, depth * 0.5f + 0.05f);
            door.transform.localScale = new Vector3(1.0f, 1.9f, 0.08f);
            door.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_WoodDark"];
            Object.DestroyImmediate(door.GetComponent<BoxCollider>());

            for (int side = -1; side <= 1; side += 2)
            {
                var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frame.name = name + (side < 0 ? "_WindowL_Frame" : "_WindowR_Frame");
                frame.transform.SetParent(building.transform, false);
                frame.transform.localPosition = new Vector3(side * width * 0.28f, 1.75f, depth * 0.5f + 0.035f);
                frame.transform.localScale = new Vector3(1.15f, 1.0f, 0.05f);
                frame.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_WoodDark"];
                Object.DestroyImmediate(frame.GetComponent<BoxCollider>());

                var glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
                glass.name = name + (side < 0 ? "_WindowL" : "_WindowR");
                glass.transform.SetParent(building.transform, false);
                glass.transform.localPosition = new Vector3(side * width * 0.28f, 1.75f, depth * 0.5f + 0.065f);
                glass.transform.localScale = new Vector3(1.0f, 0.82f, 0.03f);
                glass.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_Glass"];
                Object.DestroyImmediate(glass.GetComponent<BoxCollider>());
            }

            var awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            awning.name = name + "_Awning";
            awning.transform.SetParent(building.transform, false);
            awning.transform.localPosition = new Vector3(0f, 2.45f, depth * 0.5f + 0.55f);
            awning.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            awning.transform.localScale = new Vector3(width * 0.85f, 0.06f, 1.1f);
            awning.GetComponent<Renderer>().sharedMaterial = Materials[awningMaterials[variant % awningMaterials.Length]];
            Object.DestroyImmediate(awning.GetComponent<BoxCollider>());

            var signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = name + "_Sign";
            signBoard.transform.SetParent(building.transform, false);
            signBoard.transform.localPosition = new Vector3(0f, height - 0.55f, depth * 0.5f + 0.05f);
            signBoard.transform.localScale = new Vector3(width * 0.55f, 0.42f, 0.06f);
            signBoard.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_SignGreen"];
            Object.DestroyImmediate(signBoard.GetComponent<BoxCollider>());

            var signInner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signInner.name = name + "_SignInner";
            signInner.transform.SetParent(building.transform, false);
            signInner.transform.localPosition = new Vector3(0f, height - 0.55f, depth * 0.5f + 0.085f);
            signInner.transform.localScale = new Vector3(width * 0.45f, 0.30f, 0.03f);
            signInner.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_SignGold"];
            Object.DestroyImmediate(signInner.GetComponent<BoxCollider>());

            if (height > 4.2f)
            {
                var upperWindow = GameObject.CreatePrimitive(PrimitiveType.Cube);
                upperWindow.name = name + "_UpperWindow";
                upperWindow.transform.SetParent(building.transform, false);
                upperWindow.transform.localPosition = new Vector3(0f, height * 0.78f, depth * 0.5f + 0.04f);
                upperWindow.transform.localScale = new Vector3(width * 0.38f, 0.72f, 0.04f);
                upperWindow.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_Glass"];
                Object.DestroyImmediate(upperWindow.GetComponent<BoxCollider>());
            }
        }

        private static void CreateStreetFence(Transform root)
        {
            var railMaterial = Materials["M_Warteg_MetalDark"];
            for (int i = 0; i <= 10; i++)
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = $"StreetFence_Post_{i}";
                post.transform.SetParent(root, false);
                post.transform.localPosition = new Vector3(-10f + i * 2f, 0.55f, 9.65f);
                post.transform.localScale = new Vector3(0.08f, 1.1f, 0.08f);
                post.GetComponent<Renderer>().sharedMaterial = railMaterial;
                Object.DestroyImmediate(post.GetComponent<BoxCollider>());
            }

            var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "StreetFence_Rail";
            rail.transform.SetParent(root, false);
            rail.transform.localPosition = new Vector3(0f, 1.0f, 9.65f);
            rail.transform.localScale = new Vector3(20f, 0.06f, 0.06f);
            rail.GetComponent<Renderer>().sharedMaterial = railMaterial;
            Object.DestroyImmediate(rail.GetComponent<BoxCollider>());
        }

        private static void CreateStreetLamp(Transform root, Vector3 position)
        {
            var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "StreetLamp_Pole";
            pole.transform.SetParent(root, false);
            pole.transform.localPosition = position + new Vector3(0f, 1.8f, 0f);
            pole.transform.localScale = new Vector3(0.08f, 1.8f, 0.08f);
            pole.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_MetalDark"];
            Object.DestroyImmediate(pole.GetComponent<CapsuleCollider>());

            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "StreetLamp_Arm";
            arm.transform.SetParent(root, false);
            arm.transform.localPosition = position + new Vector3(0f, 3.55f, 0.35f);
            arm.transform.localScale = new Vector3(0.9f, 0.08f, 0.08f);
            arm.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_MetalDark"];
            Object.DestroyImmediate(arm.GetComponent<BoxCollider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "StreetLamp_Head";
            head.transform.SetParent(root, false);
            head.transform.localPosition = position + new Vector3(0f, 3.45f, 0.8f);
            head.transform.localScale = new Vector3(0.35f, 0.12f, 0.35f);
            head.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_MetalDark"];
            Object.DestroyImmediate(head.GetComponent<BoxCollider>());

            var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "StreetLamp_Bulb";
            bulb.transform.SetParent(root, false);
            bulb.transform.localPosition = position + new Vector3(0f, 3.35f, 0.8f);
            bulb.transform.localScale = Vector3.one * 0.22f;
            bulb.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_Bulb"];
            Object.DestroyImmediate(bulb.GetComponent<SphereCollider>());
        }

        private static void CreateStreetTrees(Transform root)
        {
            foreach (var x in new[] { -6.8f, 6.8f })
            {
                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = $"StreetTree_{x:0.#}";
                trunk.transform.SetParent(root, false);
                trunk.transform.localPosition = new Vector3(x, 0.85f, 7.7f);
                trunk.transform.localScale = new Vector3(0.16f, 0.75f, 0.16f);
                trunk.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_WoodDark"];

                var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.name = $"StreetTreeCrown_{x:0.#}";
                crown.transform.SetParent(root, false);
                crown.transform.localPosition = new Vector3(x, 2.05f, 7.7f);
                crown.transform.localScale = new Vector3(1.8f, 1.4f, 1.8f);
                crown.GetComponent<Renderer>().sharedMaterial = Materials["M_Warteg_Leaf"];
                Object.DestroyImmediate(crown.GetComponent<SphereCollider>());
            }
        }

        private static void AddWartegColliders(Transform warteg)
        {
            AddBox(warteg, "Collider_Floor", new Vector3(0f, 0.08f, 0f), new Vector3(8.2f, 0.16f, 6.2f));
            AddBox(warteg, "Collider_Step", new Vector3(0f, 0.06f, 3.45f), new Vector3(8.2f, 0.12f, 0.85f));
            AddBox(warteg, "Collider_BackWall", new Vector3(0f, 1.56f, -3.0f), new Vector3(8.2f, 2.8f, 0.16f));
            AddBox(warteg, "Collider_LeftWall", new Vector3(-4.05f, 1.56f, 0f), new Vector3(0.16f, 2.8f, 6.0f));
            AddBox(warteg, "Collider_RightWall", new Vector3(4.05f, 1.56f, 0f), new Vector3(0.16f, 2.8f, 6.0f));
            AddBox(warteg, "Collider_PillarL", new Vector3(-3.95f, 1.285f, 3.0f), new Vector3(0.28f, 2.25f, 0.30f));
            AddBox(warteg, "Collider_PillarR", new Vector3(3.95f, 1.285f, 3.0f), new Vector3(0.28f, 2.25f, 0.30f));
            var awningXs = new[] { -4.0f, -2.9f, 2.9f, 4.0f };
            for (int i = 0; i < awningXs.Length; i++)
                AddBox(warteg, $"Collider_AwningPost_{i}", new Vector3(awningXs[i], 1.185f, 5.35f), new Vector3(0.14f, 2.05f, 0.14f));
            AddBox(warteg, "Collider_Counter", new Vector3(0f, 0.55f, CounterZ), new Vector3(6.4f, 0.78f, 0.95f));
            AddBox(warteg, "Collider_PrepTable", new Vector3(3.3f, 0.53f, -2.35f), new Vector3(1.35f, 0.74f, 0.55f));
            AddBox(warteg, "Collider_Cashier", new Vector3(2.7f, 0.40f, 1.6f), new Vector3(1.4f, 0.80f, 0.7f));
            AddBox(warteg, "Collider_TableLong", new Vector3(-2.6f, 0.75f, 1.4f), new Vector3(0.78f, 0.08f, 2.6f));
            AddBox(warteg, "Collider_BenchWest", new Vector3(-3.25f, 0.45f, 1.4f), new Vector3(0.32f, 0.07f, 2.6f));
            AddBox(warteg, "Collider_BenchEast", new Vector3(-1.95f, 0.45f, 1.4f), new Vector3(0.32f, 0.07f, 2.6f));
            AddBox(warteg, "Collider_Cooler", new Vector3(3.45f, 1.01f, -0.3f), new Vector3(0.70f, 1.70f, 0.55f));
            AddBox(warteg, "Collider_Crates", new Vector3(3.6f, 0.51f, 2.65f), new Vector3(0.55f, 0.70f, 0.40f));
            AddBox(warteg, "Collider_CrateBlue", new Vector3(3.15f, 0.33f, 2.95f), new Vector3(0.55f, 0.35f, 0.40f));
            AddBox(warteg, "Collider_Gas", new Vector3(3.75f, 0.47f, -2.55f), new Vector3(0.32f, 0.58f, 0.32f));
        }

        private static void CreateSignage(Transform warteg)
        {
            CreateWorldText(warteg, "Sign_Warteg", "WARTEG TEGAL SARI",
                new Vector3(0f, 2.05f, 5.79f), Quaternion.Euler(0f, 180f, 0f), 0.035f, 42f,
                new Color(0.06f, 0.22f, 0.13f), TextAlignmentOptions.Center);

            CreateWorldText(warteg, "Sign_MenuTitle", "MENU HARI INI",
                new Vector3(-2.1f, 2.24f, -2.795f), Quaternion.Euler(0f, 180f, 0f), 0.036f, 32f,
                new Color(0.94f, 0.88f, 0.74f), TextAlignmentOptions.Center);

            string[] lines = { "Nasi putih", "Rendang", "Ayam goreng", "Tempe & sayur" };
            for (int i = 0; i < lines.Length; i++)
            {
                CreateWorldText(warteg, $"Sign_Menu_{i}", lines[i],
                    new Vector3(-2.1f, 2.08f - i * 0.105f, -2.795f), Quaternion.Euler(0f, 180f, 0f), 0.028f, 28f,
                    new Color(0.94f, 0.88f, 0.74f), TextAlignmentOptions.Center);
            }

            CreateWorldText(warteg, "Sign_Kasir", "KASIR",
                new Vector3(2.7f, 1.06f, 1.24f), Quaternion.Euler(0f, 180f, 0f), 0.032f, 32f,
                new Color(0.94f, 0.88f, 0.74f), TextAlignmentOptions.Center);

            for (int i = 0; i < FoodIds.Length; i++)
            {
                var x = -2.7f + i * 0.9f;
                var label = level.foodPool[i] != null ? level.foodPool[i].displayName : FoodIds[i];
                CreateWorldText(warteg, $"Label_{FoodIds[i]}", label,
                    new Vector3(x, 1.23f, -0.925f), Quaternion.Euler(0f, 180f, 0f), 0.028f, 28f,
                    new Color(0.94f, 0.88f, 0.74f), TextAlignmentOptions.Center);
            }
        }

        private static FoodStand CreateFoodDisplay(Transform warteg)
        {
            var displayObject = new GameObject("FoodDisplay");
            displayObject.transform.SetParent(warteg, false);
            var display = displayObject.AddComponent<FoodStand>();
            display.displayName = "Warteg Tegal Sari";
            display.foods = level.foodPool;
            display.anchors = new Transform[level.foodPool.Length];
            display.enabled = false;

            var foodRoot = new GameObject("Foods");
            foodRoot.transform.SetParent(warteg, false);

            for (int i = 0; i < level.foodPool.Length; i++)
            {
                var food = level.foodPool[i];
                if (food == null || food.prefab == null)
                {
                    Debug.LogError($"[Scene2Warteg] Missing food prefab at index {i}.");
                    continue;
                }

                var x = -2.7f + i * 0.9f;
                var anchor = new GameObject($"Anchor_{food.id}");
                anchor.transform.SetParent(displayObject.transform, false);
                anchor.transform.localPosition = new Vector3(x, TrayY, CounterZ);
                anchor.transform.localRotation = Quaternion.identity;
                display.anchors[i] = anchor.transform;

                var dishPath = IsBowl(food.id) ? BowlFbx : PlateFbx;
                var dish = InstantiateModel(dishPath, warteg, $"Dish_{food.id}", new Vector3(x, TrayY - 0.035f, CounterZ), Quaternion.identity);
                if (dish != null)
                    dish.transform.localScale = Vector3.one * 0.58f;

                const int portionCount = 3;
                for (int portion = 0; portion < portionCount; portion++)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(food.prefab, foodRoot.transform);
                    instance.name = portion == 0 ? food.id : $"{food.id}_{portion + 1}";
                    var spawnPosition = anchor.transform.position + anchor.transform.rotation * PortionOffset(portion, portionCount);
                    instance.transform.position = spawnPosition;
                    instance.transform.rotation = anchor.transform.rotation;
                    var item = instance.GetComponent<FoodItem>();
                    item.data = food;
                    item.SetHome(display, spawnPosition, anchor.transform.rotation);
                    instance.transform.SetParent(foodRoot.transform, true);
                }
            }

            return display;
        }

        private static GameObject CreateServingStation(Transform warteg)
        {
            var platePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServingPlatePath);
            var plateObject = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab, warteg);
            plateObject.name = "ServingPlate";
            plateObject.transform.localPosition = new Vector3(2.25f, 0.795f, 1.6f);
            plateObject.transform.localRotation = Quaternion.identity;

            var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServeStationPath);
            var stationObject = (GameObject)PrefabUtility.InstantiatePrefab(stationPrefab, warteg);
            stationObject.name = "ServeStation";
            stationObject.transform.localPosition = new Vector3(3.05f, 0.795f, 1.6f);
            stationObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            return plateObject;
        }

        private static void CreateNpcCast(Transform warteg)
        {
            var cast = new GameObject("NpcCast");
            cast.transform.SetParent(warteg, false);

            PlaceNpc(cast.transform, "NPC_Male_A", "Npc_Cook", new Vector3(-0.8f, FloorY, -2.3f), 0f, "Cook", "Wipe", true, true, 0f);
            PlaceNpc(cast.transform, "NPC_FemaleHijab_A", "Npc_Server", new Vector3(1.35f, FloorY, -2.3f), 0f, "Server", "Idle", true, false, 0.2f);
            PlaceNpc(cast.transform, "NPC_Male_B", "Npc_Diner_Male", new Vector3(-3.22f, FloorY - 0.10f, 0.85f), 90f, "Diner", "Eat", false, false, 0.15f);
            PlaceNpc(cast.transform, "NPC_Female_A", "Npc_Diner_Female", new Vector3(-3.22f, FloorY - 0.10f, 1.95f), 90f, "Diner", "Eat", false, false, 0.55f);
            PlaceNpc(cast.transform, "NPC_FemaleHijab_B", "Npc_Diner_Hijab", new Vector3(-1.98f, FloorY - 0.10f, 1.4f), -90f, "Diner", "Eat", false, false, 0.35f);
        }

        private static GameObject PlaceNpc(
            Transform parent, string prefabName, string name, Vector3 position, float yaw,
            string role, string clip, bool apron, bool cap, float phase)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{NpcFolder}/{prefabName}.prefab");
            if (prefab == null)
            {
                Debug.LogError($"[Scene2Warteg] Missing NPC prefab {prefabName}.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            NpcKitBuilder.ConfigureRole(instance, apron, cap);

            var marker = instance.GetComponent<NpcMarker>();
            if (marker != null)
            {
                marker.role = role;
                marker.seated = role == "Diner";
            }

            var animator = instance.GetComponent<NpcAnimator>();
            if (animator != null)
            {
                animator.defaultState = clip;
                animator.phaseOffset = phase;
            }

            return instance;
        }

        private static void HideNamed(Transform root, string prefix)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.StartsWith(prefix, System.StringComparison.Ordinal))
                    child.gameObject.SetActive(false);
            }
        }

        private static bool IsBowl(string foodId)
        {
            return foodId == "rendang" || foodId == "sayur_singkong";
        }

        private static Vector3 PortionOffset(int portion, int count)
        {
            if (count <= 1)
                return Vector3.zero;

            if (count == 3)
            {
                if (portion == 0)
                    return new Vector3(-0.045f, 0f, -0.028f);
                if (portion == 1)
                    return new Vector3(0.045f, 0f, -0.028f);
                return new Vector3(0f, 0f, 0.045f);
            }

            float step = 0.09f;
            return new Vector3((portion - (count - 1) * 0.5f) * step, 0f, 0f);
        }

        private static GameObject InstantiateModel(string path, Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogError($"[Scene2Warteg] Missing model: {path}");
                return null;
            }

            var wrapper = new GameObject(name);
            wrapper.transform.SetParent(parent, false);
            wrapper.transform.localPosition = localPosition;
            wrapper.transform.localRotation = localRotation;
            wrapper.transform.localScale = Vector3.one;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, wrapper.transform);
            instance.name = "Model";
            RemapMaterials(instance);
            return wrapper;
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
                    if (key == "M_Home_Ceramic")
                        key = "M_Warteg_Ceramic";

                    if (Materials.TryGetValue(key, out var replacement))
                    {
                        materials[index] = replacement;
                    }
                    else if (MissingMaterials.Add(key))
                    {
                        Debug.LogWarning($"[Scene2Warteg] No URP material for '{key}' on {renderer.name}.");
                    }
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static string NormalizeMaterialName(string name)
        {
            return name.Replace(" (Instance)", string.Empty).Split('.')[0].Trim();
        }

        private static TextMeshPro CreateWorldText(
            Transform parent, string name, string text, Vector3 localPosition, Quaternion localRotation,
            float scale, float fontSize, Color color, TextAlignmentOptions alignment)
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
            sun.transform.rotation = Quaternion.Euler(32f, -42f, 0f);
            sun.color = new Color(1f, 0.87f, 0.70f);
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            RenderSettings.sun = sun;

            var fillRoot = new GameObject("Lighting");
            fillRoot.transform.SetParent(root, false);
            AddPointLight(fillRoot.transform, "Fill_Counter", new Vector3(0f, 2.4f, -1.1f), 5f, 0.85f);
            AddPointLight(fillRoot.transform, "Fill_Table", new Vector3(-2.4f, 2.4f, 1.3f), 4.5f, 0.65f);
            AddPointLight(fillRoot.transform, "Fill_Kasir", new Vector3(2.7f, 2.3f, 1.6f), 4f, 0.55f);
            AddPointLight(fillRoot.transform, "Fill_Awning", new Vector3(0f, 2.3f, 4.6f), 5f, 0.5f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.48f, 0.42f, 0.36f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.66f, 0.56f, 0.46f);
            RenderSettings.fogStartDistance = 35f;
            RenderSettings.fogEndDistance = 110f;

            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var skyPath = "Assets/Materials/Warteg/M_Warteg_Sky.mat";
                var sky = AssetDatabase.LoadAssetAtPath<Material>(skyPath);
                if (sky == null)
                {
                    sky = new Material(skyShader);
                    AssetDatabase.CreateAsset(sky, skyPath);
                }

                sky.shader = skyShader;
                sky.SetColor("_SkyTint", new Color(0.60f, 0.58f, 0.52f));
                sky.SetColor("_GroundColor", new Color(0.30f, 0.25f, 0.20f));
                sky.SetFloat("_AtmosphereThickness", 1.05f);
                sky.SetFloat("_SunSize", 0.045f);
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
            light.color = new Color(1f, 0.76f, 0.50f);
            light.shadows = LightShadows.None;
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
                .Where(scene => File.Exists(scene.path) && scene.path != ScenePath && !scene.path.EndsWith("Level02_Padang.unity"))
                .ToList();

            var mainIndex = scenes.FindIndex(scene => scene.path == "Assets/Scenes/MainVR.unity");
            scenes.Insert(mainIndex >= 0 ? mainIndex + 1 : 0, new EditorBuildSettingsScene(ScenePath, true));

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
