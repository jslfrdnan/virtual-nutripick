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
using NutriPick.Player;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class Scene1HomeBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainVR.unity";
        private const string LevelPath = "Assets/ScriptableObjects/Levels/LD_MainVR.asset";
        private const string RulesPath = "Assets/ScriptableObjects/Rules/NutritionRules.asset";
        private const string ServingPlatePath = "Assets/Prefabs/Interaction/ServingPlate.prefab";
        private const string ServeStationPath = "Assets/Prefabs/Interaction/ServeStation.prefab";
        private const string FigmaHudPath = "Assets/Prefabs/UI/FigmaHUD.prefab";

        private const string RoomFbx = "Assets/Models/Environment/Home/KitchenRoom.fbx";
        private const string IslandFbx = "Assets/Models/Environment/Home/KitchenIsland.fbx";
        private const string DiningFbx = "Assets/Models/Environment/Home/DiningSet.fbx";
        private const string DecorFbx = "Assets/Models/Environment/Home/Decor.fbx";
        private const string PlatterFbx = "Assets/Models/Food/Plating/Platter_Oval.fbx";
        private const string PlateFbx = "Assets/Models/Food/Plating/Plate_Small.fbx";
        private const string BowlFbx = "Assets/Models/Food/Plating/Bowl_Small.fbx";

        private static readonly Dictionary<string, string> FoodFbx = new()
        {
            { "rendang", "Assets/Models/Food/Home/Food_rendang.fbx" },
            { "burger", "Assets/Models/Food/Home/Food_burger.fbx" },
            { "telur_rebus", "Assets/Models/Food/Home/Food_telur_rebus.fbx" },
            { "telur_ceplok", "Assets/Models/Food/Home/Food_telur_ceplok.fbx" },
            { "tempe", "Assets/Models/Food/Home/Food_tempe.fbx" },
            { "brokoli", "Assets/Models/Food/Home/Food_brokoli.fbx" },
            { "pisang", "Assets/Models/Food/Home/Food_pisang.fbx" },
            { "mie_goreng", "Assets/Models/Food/Home/Food_mie_goreng.fbx" },
        };

        private class MatSpec
        {
            public Color color;
            public float metallic;
            public float smoothness;
            public Color? emission;
        }

        private static readonly Dictionary<string, MatSpec> Palette = new()
        {
            { "M_Home_Wall", new MatSpec { color = new Color(0.93f, 0.92f, 0.89f), smoothness = 0.05f } },
            { "M_Home_Cabinet", new MatSpec { color = new Color(0.97f, 0.96f, 0.93f), smoothness = 0.15f } },
            { "M_Home_Counter", new MatSpec { color = new Color(0.86f, 0.85f, 0.82f), smoothness = 0.25f } },
            { "M_Home_Floor", new MatSpec { color = new Color(0.56f, 0.41f, 0.27f), smoothness = 0.15f } },
            { "M_Home_Wood", new MatSpec { color = new Color(0.58f, 0.42f, 0.26f), smoothness = 0.2f } },
            { "M_Home_BlackMetal", new MatSpec { color = new Color(0.035f, 0.035f, 0.035f), metallic = 0.7f, smoothness = 0.68f } },
            { "M_Home_Steel", new MatSpec { color = new Color(0.72f, 0.72f, 0.74f), metallic = 1f, smoothness = 0.78f } },
            { "M_Home_Glass", new MatSpec { color = new Color(0.70f, 0.85f, 0.92f), smoothness = 0.94f } },
            { "M_Home_Plant", new MatSpec { color = new Color(0.12f, 0.38f, 0.14f), smoothness = 0.4f } },
            { "M_Home_PlantLight", new MatSpec { color = new Color(0.26f, 0.55f, 0.22f), smoothness = 0.4f } },
            { "M_Home_Terracotta", new MatSpec { color = new Color(0.62f, 0.30f, 0.18f), smoothness = 0.2f } },
            { "M_Home_Ceramic", new MatSpec { color = new Color(0.97f, 0.96f, 0.93f), smoothness = 0.35f } },
            { "M_Home_Linen", new MatSpec { color = new Color(0.86f, 0.82f, 0.74f), smoothness = 0.1f } },
            { "M_Home_Rug", new MatSpec { color = new Color(0.80f, 0.77f, 0.70f), smoothness = 0.05f } },
            { "M_Home_Tile", new MatSpec { color = new Color(0.90f, 0.89f, 0.86f), smoothness = 0.2f } },
            { "M_Home_Grout", new MatSpec { color = new Color(0.72f, 0.71f, 0.68f), smoothness = 0.3f } },
            { "M_Home_Art", new MatSpec { color = new Color(0.30f, 0.42f, 0.38f), smoothness = 0.3f } },
            { "M_Home_ArtAccent", new MatSpec { color = new Color(0.78f, 0.52f, 0.28f), smoothness = 0.3f } },
            { "M_Home_Bulb", new MatSpec { color = new Color(1.0f, 0.86f, 0.62f), smoothness = 0.8f, emission = new Color(1.0f, 0.72f, 0.35f) * 2f } },
            { "M_Food_Rendang", new MatSpec { color = new Color(0.34f, 0.13f, 0.055f), smoothness = 0.45f } },
            { "M_Food_RendangSauce", new MatSpec { color = new Color(0.20f, 0.06f, 0.025f), smoothness = 0.55f } },
            { "M_Food_Bun", new MatSpec { color = new Color(0.76f, 0.48f, 0.20f), smoothness = 0.45f } },
            { "M_Food_Patty", new MatSpec { color = new Color(0.28f, 0.13f, 0.06f), smoothness = 0.4f } },
            { "M_Food_Cheese", new MatSpec { color = new Color(0.96f, 0.73f, 0.18f), smoothness = 0.6f } },
            { "M_Food_Lettuce", new MatSpec { color = new Color(0.34f, 0.64f, 0.18f), smoothness = 0.45f } },
            { "M_Food_Tomato", new MatSpec { color = new Color(0.76f, 0.12f, 0.08f), smoothness = 0.6f } },
            { "M_Food_EggWhite", new MatSpec { color = new Color(0.98f, 0.97f, 0.94f), smoothness = 0.65f } },
            { "M_Food_Yolk", new MatSpec { color = new Color(0.96f, 0.68f, 0.08f), smoothness = 0.6f } },
            { "M_Food_Tempeh", new MatSpec { color = new Color(0.78f, 0.62f, 0.38f), smoothness = 0.3f } },
            { "M_Food_TempehDark", new MatSpec { color = new Color(0.52f, 0.37f, 0.20f), smoothness = 0.2f } },
            { "M_Food_Broccoli", new MatSpec { color = new Color(0.18f, 0.46f, 0.16f), smoothness = 0.4f } },
            { "M_Food_Stem", new MatSpec { color = new Color(0.46f, 0.60f, 0.25f), smoothness = 0.35f } },
            { "M_Food_Banana", new MatSpec { color = new Color(0.95f, 0.78f, 0.18f), smoothness = 0.5f } },
            { "M_Food_BananaTip", new MatSpec { color = new Color(0.34f, 0.22f, 0.10f), smoothness = 0.2f } },
            { "M_Food_Noodle", new MatSpec { color = new Color(0.80f, 0.58f, 0.25f), smoothness = 0.45f } },
            { "M_Food_GreenOnion", new MatSpec { color = new Color(0.30f, 0.60f, 0.20f), smoothness = 0.4f } },
        };

        private static readonly HashSet<string> UnlitMaterials = new()
        {
            "M_Home_Wood", "M_Home_Floor", "M_Home_Plant", "M_Home_PlantLight",
            "M_Home_Terracotta", "M_Home_BlackMetal", "M_Home_Steel",
            "M_Home_Art", "M_Home_ArtAccent", "M_Home_Rug", "M_Home_Ceramic"
        };

        private static readonly Dictionary<string, Material> Materials = new();

        private static readonly Dictionary<string, string> FoodTextures = new()
        {
            { "M_Food_Rendang", "M_Food_Rendang_Albedo.png" },
            { "M_Food_RendangSauce", "M_Food_RendangSauce_Albedo.png" },
            { "M_Food_Bun", "M_Food_Bun_Albedo.png" },
            { "M_Food_Patty", "M_Food_Patty_Albedo.png" },
            { "M_Food_Cheese", "M_Food_Cheese_Albedo.png" },
            { "M_Food_Lettuce", "M_Food_Lettuce_Albedo.png" },
            { "M_Food_Tomato", "M_Food_Tomato_Albedo.png" },
            { "M_Food_EggWhite", "M_Food_EggWhite_Albedo.png" },
            { "M_Food_Yolk", "M_Food_Yolk_Albedo.png" },
            { "M_Food_Tempeh", "M_Food_Tempeh_Albedo.png" },
            { "M_Food_TempehDark", "M_Food_TempehDark_Albedo.png" },
            { "M_Food_Broccoli", "M_Food_Broccoli_Albedo.png" },
            { "M_Food_Stem", "M_Food_Stem_Albedo.png" },
            { "M_Food_Banana", "M_Food_Banana_Albedo.png" },
            { "M_Food_BananaTip", "M_Food_BananaTip_Albedo.png" },
            { "M_Food_Noodle", "M_Food_Noodle_Albedo.png" },
            { "M_Food_GreenOnion", "M_Food_GreenOnion_Albedo.png" },
        };

        [MenuItem("NutriPick/Build Scene 1 Home")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            ConfigureModelImporters();
            EnsureMaterials();
            CreateFoodPrefabs();
            UpdateLevel();
            BuildScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Scene1Home] Built kitchen + dining room, 8 food plates, serving plate, and serve station.");
        }

        private static void ConfigureModelImporters()
        {
            var paths = new List<string> { RoomFbx, IslandFbx, DiningFbx, DecorFbx, PlatterFbx, PlateFbx, BowlFbx };
            paths.AddRange(FoodFbx.Values);

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"[Scene1Home] Missing FBX: {path}");
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
            if (!AssetDatabase.IsValidFolder("Assets/Materials/Home"))
                AssetDatabase.CreateFolder("Assets/Materials", "Home");

            var litShader = Shader.Find("Universal Render Pipeline/Lit");
            var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (litShader == null || unlitShader == null)
            {
                Debug.LogError("[Scene1Home] URP Lit/Unlit shader not found.");
                return;
            }

            foreach (var pair in Palette)
            {
                var isUnlit = pair.Key.StartsWith("M_Food_") || UnlitMaterials.Contains(pair.Key);
                var shader = isUnlit ? unlitShader : litShader;
                var path = $"Assets/Materials/Home/{pair.Key}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, path);
                }

                material.shader = shader;
                material.SetColor("_BaseColor", pair.Value.color);
                if (isUnlit && FoodTextures.TryGetValue(pair.Key, out var textureFile))
                {
                    var texture = LoadFoodTexture(pair.Key, textureFile);
                    if (texture != null)
                    {
                        material.SetTexture("_BaseMap", texture);
                        material.SetColor("_BaseColor", Color.white);
                    }
                }

                if (!isUnlit)
                {
                    material.SetFloat("_Metallic", pair.Value.metallic);
                    material.SetFloat("_Smoothness", pair.Value.smoothness);
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

            AssetDatabase.SaveAssets();
        }

        private static Texture2D LoadFoodTexture(string materialName, string fileName)
        {
            var path = $"Assets/Textures/Food/Home/{fileName}";
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[Scene1Home] Missing food texture for {materialName}: {path}");
                return null;
            }

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 1024;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
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
                        materials[index] = replacement;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static void CreateFoodPrefabs()
        {
            foreach (var pair in FoodFbx)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(pair.Value);
                if (model == null)
                {
                    Debug.LogError($"[Scene1Home] Missing food model: {pair.Value}");
                    continue;
                }

                var root = new GameObject(pair.Key);
                var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
                modelInstance.name = "Model";
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localScale = Vector3.one;
                RemapMaterials(modelInstance);

                var item = root.AddComponent<FoodItem>();
                var body = root.AddComponent<Rigidbody>();
                body.mass = 0.2f;
                body.linearDamping = 0.4f;
                body.angularDamping = 0.4f;
                body.useGravity = true;
                body.isKinematic = true;

                var collider = root.AddComponent<BoxCollider>();
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length > 0)
                {
                    var bounds = renderers[0].bounds;
                    for (int index = 1; index < renderers.Length; index++)
                        bounds.Encapsulate(renderers[index].bounds);

                    collider.center = root.transform.InverseTransformPoint(bounds.center);
                    collider.size = bounds.size * 1.08f;
                }

                var data = AssetDatabase.LoadAssetAtPath<FoodData>($"Assets/ScriptableObjects/Food/FD_{pair.Key}.asset");
                if (data == null)
                {
                    Debug.LogError($"[Scene1Home] Missing FoodData for {pair.Key}");
                    Object.DestroyImmediate(root);
                    continue;
                }

                item.data = data;
                item.body = body;
                item.itemCollider = collider;

                var prefabPath = $"Assets/Prefabs/Food/{pair.Key}.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Object.DestroyImmediate(root);

                data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                EditorUtility.SetDirty(data);
            }

            AssetDatabase.SaveAssets();
        }

        private static void UpdateLevel()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            if (level == null)
            {
                Debug.LogError($"[Scene1Home] Missing {LevelPath}");
                return;
            }

            level.displayName = "Level 1 — Rumah";
            level.settingDescription = "Sarapan di rumah";
            level.learningFocus = new[] { "piring dasar", "pilih sayur dan protein" };
            EditorUtility.SetDirty(level);
        }

        private static void BuildScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            var rules = AssetDatabase.LoadAssetAtPath<NutritionRules>(RulesPath);

            var obsoleteNames = new HashSet<string>
            {
                "Plane", "Plane (1)", "Plane (2)", "Plane (3)",
                "TableSurfaceCollider", "TableFoodStand", "Environment", "Scene1Home",
                "FigmaHUD", "HUD", "Tray_Lauk", "Tray_Karbo",
                "LaukPlatter", "KarboSayurPlatter"
            };

            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (transform != null && obsoleteNames.Contains(transform.name))
                    Object.DestroyImmediate(transform.gameObject);
            }

            foreach (var stand in Object.FindObjectsByType<FoodStand>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (stand != null)
                    Object.DestroyImmediate(stand.gameObject);
            }

            foreach (var food in Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (food != null)
                    Object.DestroyImmediate(food.gameObject);
            }

            foreach (var oldPlate in Object.FindObjectsByType<PlateController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (oldPlate != null)
                    Object.DestroyImmediate(oldPlate.gameObject);
            }

            foreach (var oldStation in Object.FindObjectsByType<ServeStation>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (oldStation != null)
                    Object.DestroyImmediate(oldStation.gameObject);
            }

            foreach (var eventSystem in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (eventSystem != null)
                    Object.DestroyImmediate(eventSystem.gameObject);
            }

            foreach (var grid in Object.FindObjectsByType<FoodCardGridPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (grid == null)
                    continue;
                var gridRoot = grid.transform.root;
                if (gridRoot != null && gridRoot.name != "FigmaHUD")
                    Object.DestroyImmediate(gridRoot.gameObject);
            }

            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (light != null)
                    Object.DestroyImmediate(light.gameObject);
            }

            var root = new GameObject("Scene1Home");

            var room = InstantiateModel(RoomFbx, root.transform, "KitchenRoom", Vector3.zero);
            GameObject island = null;
            var dining = InstantiateModel(DiningFbx, root.transform, "DiningSet", Vector3.zero);
            ShiftDiningFurniture(dining);
            InstantiateModel(DecorFbx, root.transform, "Decor", Vector3.zero);

            AddColliders(root, room, island, dining);

            var foodPlateXs = new[] { -1.55f, -1.15f, -0.75f, -0.35f };
            var foodPlateZs = new[] { -1.30f, -1.05f };
            var plates = new FoodStand[level.foodPool.Length];
            for (int index = 0; index < level.foodPool.Length; index++)
            {
                var food = level.foodPool[index];
                if (food == null)
                    continue;

                var position = new Vector3(foodPlateXs[index % 4], 0.755f, foodPlateZs[index / 4]);
                var isBowl = food.id == "rendang" || food.id == "brokoli" || food.id == "mie_goreng";
                plates[index] = CreateFoodPlate(root.transform, food, position, isBowl);
            }

            var platePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServingPlatePath);
            var plateObject = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab, root.transform);
            plateObject.name = "ServingPlate";
            plateObject.transform.localPosition = new Vector3(-1.0f, 0.78f, -0.75f);
            plateObject.transform.localRotation = Quaternion.identity;
            var plate = plateObject.GetComponent<PlateController>();

            var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServeStationPath);
            var stationObject = (GameObject)PrefabUtility.InstantiatePrefab(stationPrefab, root.transform);
            stationObject.name = "ServeStation";
            var serveStation = stationObject.GetComponent<ServeStation>();
            serveStation.transform.localPosition = new Vector3(-0.35f, 0.78f, -0.75f);
            serveStation.transform.localRotation = Quaternion.identity;

            var hud = GameObject.Find("FigmaHUD");
            if (hud == null)
            {
                var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FigmaHudPath);
                hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
                hud.name = "FigmaHUD";
            }

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var reticle = Object.FindFirstObjectByType<ReticleInteractor>();
            var player = Object.FindFirstObjectByType<FirstPersonController>();
            if (flow == null || reticle == null || player == null)
            {
                Debug.LogError("[Scene1Home] Missing LevelFlowController, ReticleInteractor, or FirstPersonController.");
                return;
            }

            flow.level = level;
            flow.rules = rules;
            flow.plate = plate;
            flow.stands = plates;
            flow.serveStation = serveStation;
            flow.player = player;
            flow.reticle = reticle;
            flow.foodCards = hud.GetComponentInChildren<FoodCardGridPresenter>(true);
            flow.mealStatus = hud.GetComponentInChildren<MealStatusBarPresenter>(true);
            flow.bar = hud.GetComponentInChildren<NutritionBar>(true);
            flow.timer = hud.GetComponentInChildren<LevelTimerDisplay>(true);
            flow.prompt = hud.GetComponentInChildren<InteractionPrompt>(true);
            flow.infoCard = hud.GetComponentInChildren<FoodInfoCardPresenter>(true);
            flow.report = hud.GetComponentInChildren<HealthReportPresenter>(true);
            flow.finalResult = hud.GetComponentInChildren<FinalResultPresenter>(true);
            flow.recap = hud.GetComponentInChildren<RecapPresenter>(true);
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

            if (player != null)
            {
                player.transform.position = new Vector3(0.4f, 0f, -2f);
                player.transform.rotation = Quaternion.identity;
            }

            ConfigureLighting(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject InstantiateModel(string path, Transform parent, string name, Vector3 localPosition)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                Debug.LogError($"[Scene1Home] Missing model: {path}");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localScale = Vector3.one;
            RemapMaterials(instance);
            return instance;
        }

        private static void ShiftDiningFurniture(GameObject dining)
        {
            if (dining == null)
                return;

            var offset = new Vector3(-1.0f, 0f, -0.1f);
            foreach (var child in dining.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.StartsWith("Sideboard")
                    || child.name == "Chair_SouthLeft"
                    || child.name == "Chair_SouthRight"
                    || child.name == "TableVase"
                    || child.name.StartsWith("TablePlant"))
                {
                    child.gameObject.SetActive(false);
                    continue;
                }

                if (child.name.StartsWith("DiningTable")
                    || child.name.StartsWith("Chair_")
                    || child.name == "DiningRug"
                    || child.name.StartsWith("DiningPendant"))
                {
                    child.localPosition += offset;
                }
            }
        }

        private static void AddColliders(GameObject root, GameObject room, GameObject island, GameObject dining)
        {
            AddBox(root.transform, "Collider_Floor", new Vector3(0f, -0.05f, 0f), new Vector3(6f, 0.1f, 5f));
            AddBox(root.transform, "Collider_WallBack", new Vector3(0f, 1.4f, 2.45f), new Vector3(6f, 2.8f, 0.1f));
            AddBox(root.transform, "Collider_WallFront", new Vector3(0f, 1.4f, -2.45f), new Vector3(6f, 2.8f, 0.1f));
            AddBox(root.transform, "Collider_WallLeft", new Vector3(-2.95f, 1.4f, 0f), new Vector3(0.1f, 2.8f, 5f));
            AddBox(root.transform, "Collider_WallRight", new Vector3(2.95f, 1.4f, 0f), new Vector3(0.1f, 2.8f, 5f));
            AddBox(root.transform, "Collider_KitchenRun", new Vector3(0f, 0.45f, 2.05f), new Vector3(4.1f, 0.9f, 0.7f));
            AddBox(root.transform, "Collider_Fridge", new Vector3(2.4f, 1.0f, 2.05f), new Vector3(0.8f, 2.0f, 0.75f));
            AddBox(root.transform, "Collider_Sideboard", new Vector3(-2.6f, 0.4f, -1.0f), new Vector3(0.45f, 0.85f, 1.5f));

            if (island != null)
                AddBox(island.transform, "Collider_Island", new Vector3(0f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 0.9f));

            if (dining != null)
            {
                AddBox(dining.transform, "Collider_Table", new Vector3(-1.0f, 0.375f, -1.1f), new Vector3(1.6f, 0.75f, 0.9f));
                foreach (var child in dining.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.StartsWith("Chair_"))
                        AddBox(child, "Collider_Chair", new Vector3(0f, 0.475f, 0f), new Vector3(0.5f, 0.95f, 0.5f));
                }
            }
        }

        private static void AddBox(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var colliderObject = new GameObject(name);
            colliderObject.transform.SetParent(parent, false);
            var collider = colliderObject.AddComponent<BoxCollider>();
            collider.center = center;
            collider.size = size;
        }

        private static FoodStand CreateFoodPlate(Transform parent, FoodData food, Vector3 position, bool bowl)
        {
            var station = new GameObject($"FoodPlate_{food.id}");
            station.transform.SetParent(parent, false);
            station.transform.position = position;
            station.transform.rotation = Quaternion.identity;

            var stand = station.AddComponent<FoodStand>();
            stand.displayName = food.displayName;
            stand.foods = new[] { food };
            stand.anchors = new Transform[1];
            stand.enabled = false;

            var anchor = new GameObject($"FoodPlate_{food.id}_Anchor");
            anchor.transform.SetParent(parent, false);
            anchor.transform.position = new Vector3(position.x, 0.79f, position.z);
            anchor.transform.rotation = Quaternion.identity;
            stand.anchors[0] = anchor.transform;

            var dishPath = bowl ? BowlFbx : PlateFbx;
            var dish = InstantiateModel(dishPath, station.transform, "Dish", Vector3.zero);
            if (dish != null)
                dish.transform.localScale = Vector3.one * 0.7f;

            var foodRoot = new GameObject($"FoodPlate_{food.id}_Food");
            foodRoot.transform.SetParent(parent, false);

            if (food.prefab == null)
            {
                Debug.LogError($"[Scene1Home] Missing food prefab for {food.id}.");
                return stand;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(food.prefab, foodRoot.transform);
            instance.name = food.id;
            instance.transform.position = anchor.transform.position;
            instance.transform.rotation = anchor.transform.rotation;
            var item = instance.GetComponent<FoodItem>();
            item.data = food;
            item.SetHome(stand, anchor.transform.position, anchor.transform.rotation);
            instance.transform.SetParent(foodRoot.transform, true);
            return stand;
        }

        private static void ConfigureLighting(Transform root)
        {
            var directional = Object.FindFirstObjectByType<Light>();
            if (directional == null || directional.type != LightType.Directional)
            {
                var lightObject = new GameObject("Directional Light");
                lightObject.transform.SetParent(root, false);
                directional = lightObject.AddComponent<Light>();
                directional.type = LightType.Directional;
            }

            directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            directional.intensity = 0.6f;
            directional.color = new Color(1f, 0.95f, 0.88f);
            directional.shadows = LightShadows.Soft;
            directional.shadowStrength = 0.7f;
            RenderSettings.sun = directional;

            var lightRoot = new GameObject("Lighting");
            lightRoot.transform.SetParent(root, false);

            AddPointLight(lightRoot.transform, "Pendant_Dining", new Vector3(-1.0f, 1.98f, -1.1f));

            AddFillLight(lightRoot.transform, "Fill_Center", new Vector3(0f, 2.6f, 0f));
            AddFillLight(lightRoot.transform, "Fill_Left", new Vector3(-2f, 2.6f, 1f));
            AddFillLight(lightRoot.transform, "Fill_Right", new Vector3(2f, 2.6f, -1f));

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.black;
            RenderSettings.ambientIntensity = 0f;
            RenderSettings.skybox = null;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = null;

            var ambientProbe = new SphericalHarmonicsL2();
            ambientProbe.AddAmbientLight(Color.black);
            RenderSettings.ambientProbe = ambientProbe;
            DynamicGI.UpdateEnvironment();
        }

        private static void AddPointLight(Transform parent, string name, Vector3 position)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 2.5f;
            light.intensity = 0.8f;
            light.color = new Color(1f, 0.86f, 0.66f);
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.55f;
        }

        private static void AddFillLight(Transform parent, string name, Vector3 position)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 5f;
            light.intensity = 1.0f;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.shadows = LightShadows.None;
        }

        private static void UpdateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => File.Exists(scene.path))
                .ToList();

            if (scenes.All(scene => scene.path != ScenePath))
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));

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
