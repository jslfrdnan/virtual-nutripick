using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.Nutrition;
using NutriPick.Player;
using NutriPick.UI;
using NutriPick.Utilities;

namespace NutriPick.Editor
{
    public static class NutriPickBuilder
    {
        private const string FoodFolder = "Assets/ScriptableObjects/Food";
        private const string LevelFolder = "Assets/ScriptableObjects/Levels";
        private const string RulesFolder = "Assets/ScriptableObjects/Rules";
        private const string FoodPrefabFolder = "Assets/Prefabs/Food";
        private const string UIPrefabFolder = "Assets/Prefabs/UI";
        private const string InteractionPrefabFolder = "Assets/Prefabs/Interaction";
        private const string EnvironmentPrefabFolder = "Assets/Prefabs/Environment";
        private const string PlayerPrefabFolder = "Assets/Prefabs/Player";
        private const string SceneFolder = "Assets/Scenes";
        private const string MaterialFolder = "Assets/Materials";

        private static TMP_FontAsset cachedFont;

        private class FoodSpec
        {
            public string id;
            public string displayName;
            public string description;
            public FoodCompositionCategory composition;
            public ProcessingLevel processing;
            public bool isFried;
            public bool isDrink;
            public int calories;
            public float protein;
            public float carbs;
            public float fat;
            public float fiber;
            public float addedSugar;
            public float sodium;
            public string portionLabel;
            public HealthyOverride healthyOverride;
            public string fbxPath;
            public PrimitiveType? primitive;
            public Vector3 primitiveScale;
            public float targetSize;
            public Color color;
        }

        private static readonly FoodSpec[] FoodSpecs =
        {
            new FoodSpec { id = "nasi_putih", displayName = "Nasi putih", description = "Sumber karbohidrat utama. Padukan dengan sayur dan protein agar energinya lebih stabil.", composition = FoodCompositionCategory.Carbohydrate, processing = ProcessingLevel.Minimal, calories = 195, protein = 4.0f, carbs = 43.0f, fat = 0.4f, fiber = 0.6f, addedSugar = 0f, sodium = 2f, portionLabel = "150 g", color = new Color(0.95f, 0.93f, 0.85f), primitive = PrimitiveType.Cylinder, primitiveScale = new Vector3(0.14f, 0.035f, 0.14f), targetSize = 0.14f },
            new FoodSpec { id = "telur_rebus", displayName = "Telur rebus", description = "Protein lengkap dengan lemak yang lebih rendah karena tidak digoreng.", composition = FoodCompositionCategory.Protein, processing = ProcessingLevel.Minimal, calories = 78, protein = 6.3f, carbs = 0.6f, fat = 5.3f, fiber = 0f, addedSugar = 0f, sodium = 62f, portionLabel = "1 butir (55 g)", color = new Color(0.95f, 0.90f, 0.75f), fbxPath = "Assets/Models/Food/telur.fbx", targetSize = 0.08f },
            new FoodSpec { id = "telur_ceplok", displayName = "Telur ceplok", description = "Protein baik, tetapi digoreng sehingga lemak dan kalorinya lebih tinggi.", composition = FoodCompositionCategory.Protein, processing = ProcessingLevel.Processed, isFried = true, calories = 110, protein = 6.5f, carbs = 0.5f, fat = 8.5f, fiber = 0f, addedSugar = 0f, sodium = 170f, portionLabel = "1 butir (60 g)", color = new Color(0.95f, 0.82f, 0.35f), fbxPath = "Assets/Models/Food/telur ceplok.fbx", targetSize = 0.12f },
            new FoodSpec { id = "tempe", displayName = "Tempe", description = "Protein nabati dari kedelai, kaya serat dan zat besi. Pilih yang tidak terlalu banyak minyak.", composition = FoodCompositionCategory.Protein, processing = ProcessingLevel.Processed, calories = 100, protein = 9.0f, carbs = 4.0f, fat = 6.0f, fiber = 1.5f, addedSugar = 0f, sodium = 120f, portionLabel = "50 g", color = new Color(0.55f, 0.35f, 0.15f), fbxPath = "Assets/Models/Food/tempe.fbx", targetSize = 0.12f },
            new FoodSpec { id = "brokoli", displayName = "Brokoli", description = "Sayur kaya serat, vitamin C, dan folat. Tambahkan sedikit minyak agar vitamin larut lemaknya terserap.", composition = FoodCompositionCategory.Vegetable, processing = ProcessingLevel.Minimal, calories = 34, protein = 2.8f, carbs = 7.0f, fat = 0.4f, fiber = 2.6f, addedSugar = 0f, sodium = 33f, portionLabel = "100 g", color = new Color(0.20f, 0.55f, 0.20f), fbxPath = "Assets/Models/Food/brokoli.fbx", targetSize = 0.14f },
            new FoodSpec { id = "pisang", displayName = "Pisang", description = "Buah praktis dengan karbohidrat dan kalium. Cocok sebagai camilan atau pelengkap.", composition = FoodCompositionCategory.Fruit, processing = ProcessingLevel.Minimal, calories = 105, protein = 1.3f, carbs = 27.0f, fat = 0.4f, fiber = 3.1f, addedSugar = 0f, sodium = 1f, portionLabel = "1 buah (118 g)", color = new Color(0.95f, 0.80f, 0.20f), fbxPath = "Assets/Models/Food/pisang.fbx", targetSize = 0.18f },
            new FoodSpec { id = "mie_goreng", displayName = "Mie goreng", description = "Mengenyangkan, tetapi tinggi natrium dan lemak. Batasi porsinya dan tambahkan sayur.", composition = FoodCompositionCategory.Carbohydrate, processing = ProcessingLevel.UltraProcessed, isFried = true, calories = 330, protein = 8.0f, carbs = 45.0f, fat = 12.0f, fiber = 2.0f, addedSugar = 3.0f, sodium = 800f, portionLabel = "150 g", color = new Color(0.75f, 0.55f, 0.25f), fbxPath = "Assets/Models/Food/mie.fbx", targetSize = 0.16f },
            new FoodSpec { id = "kerupuk", displayName = "Kerupuk", description = "Renyah, tetapi tinggi natrium dan kalori. Nikmati dalam jumlah kecil.", composition = FoodCompositionCategory.Snack, processing = ProcessingLevel.UltraProcessed, isFried = true, calories = 150, protein = 1.0f, carbs = 20.0f, fat = 7.0f, fiber = 0.3f, addedSugar = 0f, sodium = 300f, portionLabel = "30 g", color = new Color(0.90f, 0.75f, 0.50f), primitive = PrimitiveType.Cube, primitiveScale = new Vector3(0.10f, 0.015f, 0.10f), targetSize = 0.10f },
            new FoodSpec { id = "rendang", displayName = "Rendang", description = "Protein dan rempah yang kaya rasa, tetapi tinggi lemak dan natrium. Cocok dalam porsi kecil.", composition = FoodCompositionCategory.Protein, processing = ProcessingLevel.Processed, calories = 250, protein = 15.0f, carbs = 5.0f, fat = 19.0f, fiber = 1.0f, addedSugar = 2.0f, sodium = 600f, portionLabel = "100 g", color = new Color(0.40f, 0.20f, 0.10f), fbxPath = "Assets/Models/Food/rendang.fbx", targetSize = 0.14f },
            new FoodSpec { id = "ayam_goreng", displayName = "Ayam goreng", description = "Protein baik, tetapi lapisan tepung dan minyak menambah lemak. Pilih porsi tanpa kulit bila memungkinkan.", composition = FoodCompositionCategory.Protein, processing = ProcessingLevel.Processed, isFried = true, calories = 260, protein = 20.0f, carbs = 5.0f, fat = 18.0f, fiber = 0.2f, addedSugar = 0f, sodium = 500f, portionLabel = "100 g", color = new Color(0.80f, 0.50f, 0.20f), fbxPath = "Assets/Models/Food/yamgoreng.fbx", targetSize = 0.16f },
            new FoodSpec { id = "sayur_singkong", displayName = "Sayur singkong", description = "Sayur daun singkong kaya serat dan vitamin. Santan menambah lemak, jadi jaga porsinya.", composition = FoodCompositionCategory.Vegetable, processing = ProcessingLevel.Processed, calories = 90, protein = 3.0f, carbs = 10.0f, fat = 4.0f, fiber = 4.0f, addedSugar = 0f, sodium = 300f, portionLabel = "100 g", color = new Color(0.25f, 0.50f, 0.20f), primitive = PrimitiveType.Sphere, primitiveScale = new Vector3(0.14f, 0.05f, 0.14f), targetSize = 0.14f },
            new FoodSpec { id = "burger", displayName = "Burger", description = "Praktis dan mengenyangkan, tetapi tinggi natrium, lemak, dan kalori. Padukan dengan sayur.", composition = FoodCompositionCategory.Carbohydrate, processing = ProcessingLevel.UltraProcessed, calories = 450, protein = 20.0f, carbs = 40.0f, fat = 25.0f, fiber = 2.0f, addedSugar = 8.0f, sodium = 900f, portionLabel = "1 buah (200 g)", color = new Color(0.60f, 0.35f, 0.20f), fbxPath = "Assets/Models/Food/BURGER.fbx", targetSize = 0.16f },
        };

        [MenuItem("NutriPick/Build All")]
        public static void BuildAll()
        {
            EnsureFolders();
            ConfigureQuality();
            var rules = CreateRules();
            var foods = new Dictionary<string, FoodData>();

            foreach (var spec in FoodSpecs)
            {
                var data = CreateFoodData(spec);
                var prefab = CreateFoodPrefab(spec, data);
                data.prefab = prefab;
                EditorUtility.SetDirty(data);
                foods[spec.id] = data;
            }

            AssetDatabase.SaveAssets();

            var level1 = CreateLevel("level_01_warteg", "Level 1 — Warteg", "MainVR", "Level02_Warteg", 1,
                "Sarapan di warteg", new[] { "rendang", "burger", "telur_rebus", "telur_ceplok", "tempe", "brokoli", "pisang", "mie_goreng" },
                4, false, 1, false, new[] { "piring dasar", "pilih sayur dan protein" }, foods);
            var level3 = CreateLevel("level_03_bazaar", "Level 3 — Bazaar", "Level03_Bazaar", string.Empty, 3,
                "Makan malam di bazaar", new[] { "mie_goreng", "burger", "telur_rebus", "tempe", "brokoli", "pisang" },
                4, false, 2, true, new[] { "variasi", "pilihan seimbang" }, foods);

            AssetDatabase.SaveAssets();

            CreatePlayerRig();
            CreateHud(rules);
            CreatePlate();
            CreateFoodStand();
            CreateServeStation();

            Scene2WartegBuilder.Build();
            Scene3BazaarBuilder.Build();

            AddScenesToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NutriPick] Build complete: 12 foods, 3 levels (MainVR + 2 generated scenes).");
        }

        [MenuItem("NutriPick/Build Windows")]
        public static void BuildWindows()
        {
            Directory.CreateDirectory("Builds/Windows");
            var options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    $"{SceneFolder}/MainVR.unity",
                    $"{SceneFolder}/Level02_Warteg.unity",
                    $"{SceneFolder}/Level03_Bazaar.unity"
                },
                locationPathName = "Builds/Windows/NutriPick.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[NutriPick] Build result: {report.summary.result}, size={report.summary.totalSize} bytes");
        }

        [MenuItem("NutriPick/Validate All")]
        public static void ValidateAll()
        {
            int errors = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:FoodData", new[] { FoodFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var food = AssetDatabase.LoadAssetAtPath<FoodData>(path);
                if (food == null)
                {
                    errors++;
                    Debug.LogError($"[NutriPick] Food invalid: {path} — asset could not be loaded");
                    continue;
                }

                if (!food.IsValid(out var error))
                {
                    errors++;
                    Debug.LogError($"[NutriPick] Food invalid: {path} — {error}");
                }
            }

            foreach (var guid in AssetDatabase.FindAssets("t:LevelDefinition", new[] { LevelFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (level == null)
                {
                    errors++;
                    Debug.LogError($"[NutriPick] Level invalid: {path} — asset could not be loaded");
                    continue;
                }

                if (!level.IsValid(out var error))
                {
                    errors++;
                    Debug.LogError($"[NutriPick] Level invalid: {path} — {error}");
                }
            }

            Debug.Log(errors == 0 ? "[NutriPick] Validation passed." : $"[NutriPick] Validation found {errors} error(s).");
        }

        private static void EnsureFolders()
        {
            foreach (var folder in new[]
                     {
                         FoodFolder, LevelFolder, RulesFolder, FoodPrefabFolder, UIPrefabFolder,
                         InteractionPrefabFolder, EnvironmentPrefabFolder, PlayerPrefabFolder, SceneFolder, MaterialFolder,
                         "Assets/Input"
                     })
            {
                EnsureFolder(folder);
            }
        }

        private static void ConfigureQuality()
        {
            QualitySettings.antiAliasing = 4;
            QualitySettings.vSyncCount = 0;
        }

        private static void CreateInputActions()
        {
            var path = "Assets/Input/NutriPick.inputactions";
            if (AssetDatabase.LoadAssetAtPath<InputActionAsset>(path) != null)
                return;

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "NutriPick";

            var gameplay = asset.AddActionMap("Gameplay");
            var move = gameplay.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Keyboard>/w");
            move.AddBinding("<Keyboard>/a");
            move.AddBinding("<Keyboard>/s");
            move.AddBinding("<Keyboard>/d");

            var look = gameplay.AddAction("Look", InputActionType.Value, expectedControlLayout: "Vector2");
            look.AddBinding("<Gamepad>/rightStick");
            look.AddBinding("<Mouse>/delta");

            gameplay.AddAction("Interact", InputActionType.Button, "<Gamepad>/buttonSouth");
            gameplay.AddAction("InteractKeyboard", InputActionType.Button, "<Keyboard>/enter");
            gameplay.AddAction("Cancel", InputActionType.Button, "<Gamepad>/buttonEast");
            gameplay.AddAction("Inspect", InputActionType.Button, "<Gamepad>/buttonWest");
            gameplay.AddAction("Serve", InputActionType.Button, "<Gamepad>/start");
            gameplay.AddAction("Pause", InputActionType.Button, "<Gamepad>/select");

            var menu = asset.AddActionMap("Menu");
            menu.AddAction("Navigate", InputActionType.Value, "<Gamepad>/dpad", expectedControlLayout: "Vector2");
            menu.AddAction("Submit", InputActionType.Button, "<Gamepad>/buttonSouth");
            menu.AddAction("Cancel", InputActionType.Button, "<Gamepad>/buttonEast");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
                return;
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static Material MakeMaterial(string name, Color color, bool emissive = false)
        {
            var path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2f);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static FoodData CreateFoodData(FoodSpec spec)
        {
            var path = $"{FoodFolder}/FD_{spec.id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<FoodData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<FoodData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.id = spec.id;
            data.displayName = spec.displayName;
            data.description = spec.description;
            data.composition = spec.composition;
            data.processing = spec.processing;
            data.isFried = spec.isFried;
            data.isDrink = spec.isDrink;
            data.calories = spec.calories;
            data.protein = spec.protein;
            data.carbs = spec.carbs;
            data.fat = spec.fat;
            data.fiber = spec.fiber;
            data.addedSugar = spec.addedSugar;
            data.sodium = spec.sodium;
            data.portionLabel = spec.portionLabel;
            data.healthyOverride = spec.healthyOverride;
            EditorUtility.SetDirty(data);
            return data;
        }

        private static GameObject CreateFoodPrefab(FoodSpec spec, FoodData data)
        {
            var material = MakeMaterial("Food_" + spec.id, spec.color);
            var root = new GameObject(spec.id);
            var item = root.AddComponent<FoodItem>();
            item.data = data;
            var body = root.AddComponent<Rigidbody>();
            body.mass = 0.3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            GameObject visual;
            if (!string.IsNullOrEmpty(spec.fbxPath))
            {
                var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(spec.fbxPath);
                if (fbx != null)
                {
                    visual = UnityEngine.Object.Instantiate(fbx);
                    visual.name = "Model";
                    visual.transform.SetParent(root.transform, false);
                    foreach (var collider in visual.GetComponentsInChildren<Collider>())
                        UnityEngine.Object.DestroyImmediate(collider);
                    foreach (var rb in visual.GetComponentsInChildren<Rigidbody>())
                        UnityEngine.Object.DestroyImmediate(rb);
                    NormalizeVisual(visual, spec.targetSize);
                }
                else
                {
                    visual = CreatePrimitiveVisual(spec.primitive ?? PrimitiveType.Cube, spec.primitiveScale, material);
                    visual.transform.SetParent(root.transform, false);
                }
            }
            else
            {
                visual = CreatePrimitiveVisual(spec.primitive ?? PrimitiveType.Cube, spec.primitiveScale, material);
                visual.transform.SetParent(root.transform, false);
            }

            var bounds = CalculateBounds(root);
            var box = root.AddComponent<BoxCollider>();
            box.center = bounds.center;
            box.size = bounds.size;
            item.body = body;
            item.itemCollider = box;

            var path = $"{FoodPrefabFolder}/{spec.id}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreatePrimitiveVisual(PrimitiveType type, Vector3 scale, Material material)
        {
            var visual = GameObject.CreatePrimitive(type);
            visual.name = "Model";
            var collider = visual.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
            visual.GetComponent<Renderer>().sharedMaterial = material;
            visual.transform.localScale = scale;
            visual.transform.localPosition = new Vector3(0f, scale.y * 0.5f, 0f);
            return visual;
        }

        private static void NormalizeVisual(GameObject visual, float targetSize)
        {
            var renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            float largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (largest > 0.001f)
                visual.transform.localScale *= targetSize / largest;

            bounds = CalculateBounds(visual);
            visual.transform.localPosition += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.one * 0.1f);

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        private static NutritionRules CreateRules()
        {
            var path = $"{RulesFolder}/NutritionRules.asset";
            var rules = AssetDatabase.LoadAssetAtPath<NutritionRules>(path);
            if (rules == null)
            {
                rules = ScriptableObject.CreateInstance<NutritionRules>();
                AssetDatabase.CreateAsset(rules, path);
            }
            EditorUtility.SetDirty(rules);
            return rules;
        }

        private static LevelDefinition CreateLevel(
            string id, string displayName, string sceneName, string nextSceneName, int index,
            string setting, string[] foodIds, int minItems, bool requiresDrink, int minimumStands,
            bool final, string[] focus, Dictionary<string, FoodData> foods)
        {
            var path = $"{LevelFolder}/LD_{sceneName}.asset";
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelDefinition>();
                AssetDatabase.CreateAsset(level, path);
            }

            level.levelId = id;
            level.displayName = displayName;
            level.sceneName = sceneName;
            level.nextSceneName = nextSceneName;
            level.levelIndex = index;
            level.settingDescription = setting;
            level.foodPool = foodIds.Select(foodId => foods[foodId]).ToArray();
            level.minItems = minItems;
            level.requiresDrink = requiresDrink;
            level.minimumStands = minimumStands;
            level.isFinalLevel = final;
            level.learningFocus = focus;
            EditorUtility.SetDirty(level);
            return level;
        }

        private static GameObject CreatePlayerRig()
        {
            var root = new GameObject("PlayerRig");
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            var input = root.AddComponent<GamepadInput>();
            var fpc = root.AddComponent<FirstPersonController>();
            var carrier = root.AddComponent<HeldFoodCarrier>();
            var reticle = root.AddComponent<ReticleInteractor>();

            var pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100f;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.SetParent(pivot.transform, false);
            cameraGo.transform.localPosition = Vector3.zero;

            var hold = new GameObject("HoldAnchor");
            hold.transform.SetParent(cameraGo.transform, false);
            hold.transform.localPosition = new Vector3(0f, -0.15f, 0.5f);

            fpc.controller = controller;
            fpc.cameraPivot = pivot.transform;
            fpc.input = input;
            carrier.holdAnchor = hold.transform;
            reticle.interactionCamera = camera;
            reticle.carrier = carrier;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PlayerPrefabFolder}/PlayerRig.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreatePlate()
        {
            var root = new GameObject("Plate");
            var plate = root.AddComponent<PlateController>();
            var plateMaterial = MakeMaterial("Plate", new Color(0.95f, 0.95f, 0.95f));

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "PlateVisual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(0.26f, 0.01f, 0.26f);
            visual.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            var visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
                UnityEngine.Object.DestroyImmediate(visualCollider);
            visual.GetComponent<Renderer>().sharedMaterial = plateMaterial;

            var slots = new List<PlateSlot>();
            var positions = new[]
            {
                new Vector3(0.06f, 0.02f, 0.06f),
                new Vector3(-0.06f, 0.02f, 0.06f),
                new Vector3(0.06f, 0.02f, -0.06f),
                new Vector3(-0.06f, 0.02f, -0.06f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var slotGo = new GameObject("Slot" + (i + 1));
                slotGo.transform.SetParent(root.transform, false);
                slotGo.transform.localPosition = positions[i];
                var slot = slotGo.AddComponent<PlateSlot>();
                var trigger = slotGo.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(0.08f, 0.04f, 0.08f);
                trigger.center = new Vector3(0f, 0.02f, 0f);

                var snap = new GameObject("Snap");
                snap.transform.SetParent(slotGo.transform, false);
                snap.transform.localPosition = new Vector3(0f, 0.03f, 0f);
                slot.snapPoint = snap.transform;
                slot.trigger = trigger;
                slot.plate = plate;

                var highlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                highlight.name = "Highlight";
                highlight.transform.SetParent(slotGo.transform, false);
                highlight.transform.localScale = new Vector3(0.09f, 0.001f, 0.09f);
                highlight.transform.localPosition = new Vector3(0f, 0.006f, 0f);
                var highlightCollider = highlight.GetComponent<Collider>();
                if (highlightCollider != null)
                    UnityEngine.Object.DestroyImmediate(highlightCollider);
                highlight.GetComponent<Renderer>().sharedMaterial = MakeMaterial("SlotHighlight", new Color(0.2f, 0.9f, 0.3f), true);
                highlight.SetActive(false);
                slot.highlight = highlight;

                slots.Add(slot);
            }

            plate.slots = slots.ToArray();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{InteractionPrefabFolder}/Plate.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateFoodStand()
        {
            var root = new GameObject("FoodStand");
            var stand = root.AddComponent<FoodStand>();
            var counter = MakeMaterial("Counter", new Color(0.45f, 0.28f, 0.15f));
            var signMat = MakeMaterial("StandSign", new Color(0.12f, 0.45f, 0.25f));

            var counterGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            counterGo.name = "Counter";
            counterGo.transform.SetParent(root.transform, false);
            counterGo.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            counterGo.transform.localScale = new Vector3(1.2f, 0.9f, 0.6f);
            counterGo.GetComponent<Renderer>().sharedMaterial = counter;

            var signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = "SignBoard";
            signBoard.transform.SetParent(root.transform, false);
            signBoard.transform.localPosition = new Vector3(0f, 1.8f, -0.32f);
            signBoard.transform.localScale = new Vector3(1.2f, 0.35f, 0.05f);
            signBoard.GetComponent<Renderer>().sharedMaterial = signMat;
            var signCollider = signBoard.GetComponent<Collider>();
            if (signCollider != null)
                UnityEngine.Object.DestroyImmediate(signCollider);

            var signGo = new GameObject("SignText", typeof(TextMeshPro));
            signGo.transform.SetParent(root.transform, false);
            signGo.transform.localPosition = new Vector3(0f, 1.8f, -0.36f);
            signGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            signGo.transform.localScale = new Vector3(-0.01f, 0.01f, 0.01f);
            var signText = signGo.GetComponent<TextMeshPro>();
            signText.font = GetFont();
            signText.fontSize = 28f;
            signText.enableAutoSizing = true;
            signText.fontSizeMin = 10f;
            signText.fontSizeMax = 28f;
            signText.text = "Stand";
            signText.color = Color.white;
            signText.alignment = TextAlignmentOptions.Center;
            signText.rectTransform.sizeDelta = new Vector2(120f, 20f);
            stand.signText = signText;

            var anchors = new List<Transform>();
            for (int i = 0; i < 8; i++)
            {
                var anchor = new GameObject("Anchor" + (i + 1));
                anchor.transform.SetParent(root.transform, false);
                anchor.transform.localPosition = new Vector3(-0.5f + i * (1.0f / 7f), 0.95f, 0f);
                anchors.Add(anchor.transform);
            }
            stand.anchors = anchors.ToArray();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{EnvironmentPrefabFolder}/FoodStand.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateServeStation()
        {
            var root = new GameObject("ServeStation");
            var serve = root.AddComponent<ServeStation>();
            var dark = MakeMaterial("ServeBase", new Color(0.20f, 0.20f, 0.20f));
            var green = MakeMaterial("ServeButton", new Color(0.18f, 0.49f, 0.20f));

            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseGo.name = "Base";
            baseGo.transform.SetParent(root.transform, false);
            baseGo.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            baseGo.transform.localScale = new Vector3(0.3f, 0.08f, 0.3f);
            baseGo.GetComponent<Renderer>().sharedMaterial = dark;
            var baseCollider = baseGo.GetComponent<Collider>();
            if (baseCollider != null)
                UnityEngine.Object.DestroyImmediate(baseCollider);

            var buttonGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buttonGo.name = "Button";
            buttonGo.transform.SetParent(root.transform, false);
            buttonGo.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            buttonGo.transform.localScale = new Vector3(0.18f, 0.05f, 0.18f);
            buttonGo.GetComponent<Renderer>().sharedMaterial = green;
            var buttonCollider = buttonGo.GetComponent<Collider>();
            if (buttonCollider != null)
                UnityEngine.Object.DestroyImmediate(buttonCollider);

            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.1f, 0f);
            collider.size = new Vector3(0.3f, 0.2f, 0.3f);

            var signGo = new GameObject("ServeText", typeof(TextMeshPro));
            signGo.transform.SetParent(root.transform, false);
            signGo.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            signGo.transform.localScale = Vector3.one * 0.01f;
            var text = signGo.GetComponent<TextMeshPro>();
            text.font = GetFont();
            text.fontSize = 36f;
            text.text = "SAJIKAN";
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(20f, 4f);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{InteractionPrefabFolder}/ServeStation.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject CreateHud(NutritionRules rules)
        {
            var hud = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var eventGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventGo.transform.SetParent(hud.transform, false);

            var dark = new Color(0.12f, 0.14f, 0.16f, 0.92f);
            var light = new Color(0.98f, 0.98f, 0.98f, 1f);
            var ink = new Color(0.13f, 0.13f, 0.13f, 1f);
            var muted = new Color(0.38f, 0.38f, 0.38f, 1f);
            var green = new Color(0.18f, 0.49f, 0.20f, 1f);

            var reticle = CreateUIRoot("Reticle", hud.transform, Center, Center, Center, Vector2.zero, new Vector2(8f, 8f));
            AddPanel(reticle, Color.white, false);

            var barGo = CreateUIRoot("NutritionBar", hud.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(720f, 80f));
            AddPanel(barGo, dark, false);
            AddText(barGo.transform, "Label", "Keseimbangan Gizi", 22, Color.white, TextAlignmentOptions.Left, new Vector2(-300f, 25f), new Vector2(300f, 30f));
            var track = CreateUIRoot("Track", barGo.transform, Center, Center, Center, new Vector2(0f, -8f), new Vector2(680f, 28f));
            AddPanel(track, new Color(0.22f, 0.27f, 0.31f), false);
            var fillGo = CreateUIRoot("Fill", barGo.transform, Center, Center, Center, new Vector2(0f, -8f), new Vector2(680f, 28f));
            var fill = AddPanel(fillGo, green, false);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0.5f;
            var valueText = AddText(barGo.transform, "Value", "50", 36, Color.white, TextAlignmentOptions.Right, new Vector2(300f, 18f), new Vector2(80f, 40f));
            var bandText = AddText(barGo.transform, "Band", "Belum dinilai", 22, Color.white, TextAlignmentOptions.Right, new Vector2(300f, -18f), new Vector2(180f, 30f));
            var countsText = AddText(barGo.transform, "Counts", "Sehat 0 · Kurang sehat 0", 22, Color.white, TextAlignmentOptions.Left, new Vector2(-300f, -28f), new Vector2(360f, 24f));
            var bar = barGo.AddComponent<NutritionBar>();
            bar.fill = fill;
            bar.valueText = valueText;
            bar.labelText = bandText;
            bar.countsText = countsText;

            var promptGo = CreateUIRoot("Prompt", hud.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 56f), new Vector2(640f, 44f));
            AddPanel(promptGo, new Color(0.08f, 0.08f, 0.08f, 0.85f), false);
            var promptText = AddText(promptGo.transform, "PromptText", string.Empty, 24, Color.white, TextAlignmentOptions.Center, Vector2.zero, new Vector2(620f, 44f));
            var prompt = promptGo.AddComponent<InteractionPrompt>();
            prompt.root = promptGo;
            prompt.text = promptText;
            promptGo.SetActive(false);

            var infoRoot = CreateUIRoot("InfoCard", hud.transform, Center, Center, Center, Vector2.zero, Vector2.zero);
            var info = infoRoot.AddComponent<FoodInfoCardPresenter>();
            info.rules = rules;

            var compact = CreateUIRoot("Compact", infoRoot.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-48f, 0f), new Vector2(320f, 96f));
            AddPanel(compact, dark, false);
            info.compactName = AddText(compact.transform, "CompactName", string.Empty, 24, Color.white, TextAlignmentOptions.Left, new Vector2(-90f, 20f), new Vector2(200f, 30f));
            var compactBadgeBg = CreateUIRoot("CompactBadgeBg", compact.transform, Center, Center, Center, new Vector2(100f, 20f), new Vector2(120f, 30f));
            info.compactBadgeBackground = AddPanel(compactBadgeBg, green, false);
            info.compactBadge = AddText(compact.transform, "CompactBadge", string.Empty, 20, Color.white, TextAlignmentOptions.Center, new Vector2(100f, 20f), new Vector2(120f, 30f));
            info.compactCalories = AddText(compact.transform, "CompactCalories", string.Empty, 20, Color.white, TextAlignmentOptions.Left, new Vector2(-90f, -15f), new Vector2(200f, 24f));
            info.compactRoot = compact;
            compact.SetActive(false);

            var full = CreateUIRoot("Full", infoRoot.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(48f, 0f), new Vector2(420f, 560f));
            AddPanel(full, light, false);
            info.fullName = AddText(full.transform, "FullName", string.Empty, 28, ink, TextAlignmentOptions.Left, new Vector2(0f, 230f), new Vector2(380f, 40f));
            info.fullPortion = AddText(full.transform, "FullPortion", string.Empty, 22, muted, TextAlignmentOptions.Left, new Vector2(0f, 195f), new Vector2(380f, 28f));
            var fullBadgeBg = CreateUIRoot("FullBadgeBg", full.transform, Center, Center, Center, new Vector2(-110f, 150f), new Vector2(160f, 32f));
            info.fullBadgeBackground = AddPanel(fullBadgeBg, green, false);
            info.fullBadge = AddText(full.transform, "FullBadge", string.Empty, 20, Color.white, TextAlignmentOptions.Center, new Vector2(-110f, 150f), new Vector2(160f, 32f));
            info.fullStats = AddText(full.transform, "FullStats", string.Empty, 22, ink, TextAlignmentOptions.TopLeft, new Vector2(0f, 45f), new Vector2(380f, 180f));
            info.fullDescription = AddText(full.transform, "FullDescription", string.Empty, 22, muted, TextAlignmentOptions.TopLeft, new Vector2(0f, -120f), new Vector2(380f, 130f));
            AddText(full.transform, "FullFooter", "Nilai gizi adalah perkiraan.", 18, muted, TextAlignmentOptions.Center, new Vector2(0f, -245f), new Vector2(380f, 30f));
            info.fullRoot = full;
            full.SetActive(false);

            var reportController = CreateUIRoot("ReportController", hud.transform, Zero, One, Center, Vector2.zero, Vector2.zero);
            var report = reportController.AddComponent<HealthReportPresenter>();
            report.root = CreateReport(hud.transform, out var reportRefs);
            report.titleText = reportRefs.title;
            report.scoreText = reportRefs.score;
            report.bandText = reportRefs.band;
            report.strengthsText = reportRefs.strengths;
            report.improvementText = reportRefs.improvement;
            report.dimensionsText = reportRefs.dimensions;
            report.warningText = reportRefs.warning;
            report.disclaimerText = reportRefs.disclaimer;
            report.retryButton = reportRefs.retry;
            report.nextButton = reportRefs.next;

            var pauseController = CreateUIRoot("PauseController", hud.transform, Zero, One, Center, Vector2.zero, Vector2.zero);
            CreatePause(hud.transform);

            hud.AddComponent<SfxPlayer>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(hud, $"{UIPrefabFolder}/HUD.prefab");
            UnityEngine.Object.DestroyImmediate(hud);
            return prefab;
        }

        private class ReportRefs
        {
            public TMP_Text title;
            public TMP_Text score;
            public TMP_Text band;
            public TMP_Text strengths;
            public TMP_Text improvement;
            public TMP_Text dimensions;
            public TMP_Text warning;
            public TMP_Text disclaimer;
            public Button retry;
            public Button next;
        }

        private static GameObject CreateReport(Transform parent, out ReportRefs refs)
        {
            var root = CreateModal(parent, "ReportRoot", new Vector2(1200f, 760f));
            var panel = FindDeepChild(root.transform, "Panel");
            var ink = new Color(0.13f, 0.13f, 0.13f, 1f);
            var muted = new Color(0.38f, 0.38f, 0.38f, 1f);
            var red = new Color(0.78f, 0.16f, 0.16f, 1f);

            refs = new ReportRefs
            {
                title = AddText(panel, "Title", "Laporan Level", 44, ink, TextAlignmentOptions.Center, new Vector2(0f, 310f), new Vector2(1000f, 60f)),
                score = AddText(panel, "Score", "0", 72, ink, TextAlignmentOptions.Center, new Vector2(-400f, 190f), new Vector2(200f, 100f)),
                band = AddText(panel, "Band", "Cukup", 28, muted, TextAlignmentOptions.Center, new Vector2(-400f, 120f), new Vector2(240f, 40f)),
                strengths = AddText(panel, "Strengths", string.Empty, 24, ink, TextAlignmentOptions.TopLeft, new Vector2(100f, 180f), new Vector2(700f, 140f)),
                improvement = AddText(panel, "Improvement", string.Empty, 24, ink, TextAlignmentOptions.TopLeft, new Vector2(100f, 50f), new Vector2(700f, 60f)),
                dimensions = AddText(panel, "Dimensions", string.Empty, 22, ink, TextAlignmentOptions.TopLeft, new Vector2(0f, -80f), new Vector2(1000f, 160f)),
                warning = AddText(panel, "Warning", string.Empty, 22, red, TextAlignmentOptions.Center, new Vector2(0f, -190f), new Vector2(1000f, 40f)),
                disclaimer = AddText(panel, "Disclaimer", "Ini adalah game edukasi, bukan nasihat medis. Nilai gizi adalah perkiraan.", 18, muted, TextAlignmentOptions.Center, new Vector2(0f, -240f), new Vector2(1000f, 40f)),
                retry = CreateButton(panel, "RetryButton", "Ulangi level", new Vector2(-250f, -320f), new Vector2(220f, 56f)),
                next = CreateButton(panel, "NextButton", "Lanjut", new Vector2(0f, -320f), new Vector2(220f, 56f))
            };
            return root;
        }

        private static GameObject CreatePause(Transform parent)
        {
            var root = CreateModal(parent, "PausePanel", new Vector2(560f, 360f));
            var panel = FindDeepChild(root.transform, "Panel");
            var ink = new Color(0.13f, 0.13f, 0.13f, 1f);
            var muted = new Color(0.38f, 0.38f, 0.38f, 1f);

            AddText(panel, "Title", "Jeda", 44, ink, TextAlignmentOptions.Center, new Vector2(0f, 110f), new Vector2(480f, 60f));
            AddText(panel, "PauseMessage", "Jeda", 22, muted, TextAlignmentOptions.Center, new Vector2(0f, 40f), new Vector2(480f, 80f));
            CreateButton(panel, "ResumeButton", "Lanjut", new Vector2(-120f, -80f), new Vector2(200f, 56f));
            CreateButton(panel, "QuitButton", "Keluar", new Vector2(120f, -80f), new Vector2(200f, 56f));
            return root;
        }

        private static GameObject CreateModal(Transform parent, string name, Vector2 size)
        {
            var root = CreateUIRoot(name, parent, Zero, One, Center, Vector2.zero, Vector2.zero);
            AddPanel(root, new Color(0f, 0f, 0f, 0.55f), true);
            var panel = CreateUIRoot("Panel", root.transform, Center, Center, Center, Vector2.zero, size);
            AddPanel(panel, new Color(0.98f, 0.98f, 0.98f, 1f), true);
            root.SetActive(false);
            return root;
        }

        private static GameObject CreateUIRoot(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
                go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return go;
        }

        private static Image AddPanel(GameObject go, Color color, bool raycast)
        {
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        private static TextMeshProUGUI AddText(Transform parent, string name, string text, int fontSize, Color color, TextAlignmentOptions alignment, Vector2 position, Vector2 size)
        {
            var go = CreateUIRoot(name, parent, Center, Center, Center, position, size);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = GetFont();
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.text = text;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size)
        {
            var go = CreateUIRoot(name, parent, Center, Center, Center, position, size);
            var image = AddPanel(go, new Color(0.18f, 0.49f, 0.20f, 1f), true);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.Automatic;
            button.navigation = navigation;

            var labelGo = CreateUIRoot("Label", go.transform, Zero, One, Center, Vector2.zero, Vector2.zero);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.font = GetFont();
            tmp.fontSize = 26;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = label;
            tmp.raycastTarget = false;
            return button;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material, bool keepCollider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!keepCollider)
            {
                var collider = go.GetComponent<Collider>();
                if (collider != null)
                    UnityEngine.Object.DestroyImmediate(collider);
            }
            return go;
        }

        private static TextMeshPro CreateWorldText(Transform parent, string name, string text, Vector3 position, Quaternion rotation, float scale, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(TextMeshPro));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = new Vector3(-scale, scale, scale);
            var tmp = go.GetComponent<TextMeshPro>();
            tmp.font = GetFont();
            tmp.fontSize = fontSize;
            tmp.text = text;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = new Vector2(20f, 4f);
            return tmp;
        }

        private static void CreateLevelScene(
            LevelDefinition level, NutritionRules rules, Dictionary<string, FoodData> foods,
            GameObject playerPrefab, GameObject hudPrefab, GameObject platePrefab, GameObject standPrefab, GameObject servePrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.50f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var env = new GameObject(level.displayName + "_Environment").transform;
            if (level.levelIndex == 3)
                CreateBazaar(env, level);
            else
                CreateRoom(env, level);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            player.transform.position = level.levelIndex == 3 ? new Vector3(0f, 0f, -2.5f) : Vector3.zero;

            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            var plate = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab);
            plate.transform.position = level.levelIndex == 3 ? new Vector3(0f, 0.77f, 0f) : new Vector3(0f, 0.77f, 1.6f);

            var serve = (GameObject)PrefabUtility.InstantiatePrefab(servePrefab);
            serve.transform.position = plate.transform.position + new Vector3(0.45f, 0.01f, 0f);

            var stands = new List<FoodStand>();
            if (level.levelIndex == 3)
            {
                AddStand(stands, standPrefab, foods, "Mie & Jajanan", new[] { "mie_goreng", "burger" }, new Vector3(0f, 0f, 5f), Quaternion.Euler(0f, 0f, 0f));
                AddStand(stands, standPrefab, foods, "Lauk", new[] { "telur_rebus", "tempe" }, new Vector3(5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
                AddStand(stands, standPrefab, foods, "Sayur & Buah", new[] { "brokoli", "pisang" }, new Vector3(0f, 0f, -5f), Quaternion.Euler(0f, 180f, 0f));
            }
            else
            {
                var stand = (GameObject)PrefabUtility.InstantiatePrefab(standPrefab);
                stand.transform.position = new Vector3(0f, 0f, 2.8f);
                stand.transform.rotation = Quaternion.identity;
                var component = stand.GetComponent<FoodStand>();
                component.displayName = level.displayName;
                component.foods = level.foodPool;
                component.signText = FindDeepChild(stand.transform, "SignText")?.GetComponent<TMP_Text>();
                stands.Add(component);
            }

            var flowGo = new GameObject("LevelFlow");
            var flow = flowGo.AddComponent<LevelFlowController>();
            flow.level = level;
            flow.rules = rules;
            flow.plate = plate.GetComponent<PlateController>();
            flow.stands = stands.ToArray();
            flow.serveStation = serve.GetComponent<ServeStation>();
            flow.player = player.GetComponent<FirstPersonController>();
            flow.input = player.GetComponent<GamepadInput>();
            flow.reticle = player.GetComponent<ReticleInteractor>();
            flow.bar = hud.GetComponentInChildren<NutritionBar>(true);
            flow.prompt = hud.GetComponentInChildren<InteractionPrompt>(true);
            flow.infoCard = hud.GetComponentInChildren<FoodInfoCardPresenter>(true);
            flow.report = hud.GetComponentInChildren<HealthReportPresenter>(true);
            flow.pausePanel = FindDeepChild(hud.transform, "PausePanel")?.gameObject;
            flow.pauseMessage = FindDeepChild(hud.transform, "PauseMessage")?.GetComponent<TMP_Text>();
            flow.pauseResumeButton = FindDeepChild(hud.transform, "ResumeButton")?.GetComponent<Button>();
            flow.pauseQuitButton = FindDeepChild(hud.transform, "QuitButton")?.GetComponent<Button>();

            if (flow.reticle != null)
            {
                flow.reticle.flow = flow;
                flow.reticle.plate = flow.plate;
                flow.reticle.serveStation = flow.serveStation;
                flow.reticle.infoCard = flow.infoCard;
                flow.reticle.prompt = flow.prompt;
            }

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/{level.sceneName}.unity");
        }

        private static void AddStand(List<FoodStand> stands, GameObject standPrefab, Dictionary<string, FoodData> foods, string displayName, string[] foodIds, Vector3 position, Quaternion rotation)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(standPrefab);
            go.transform.position = position;
            go.transform.rotation = rotation;
            var stand = go.GetComponent<FoodStand>();
            stand.displayName = displayName;
            stand.foods = foodIds.Select(id => foods[id]).ToArray();
            stand.signText = FindDeepChild(go.transform, "SignText")?.GetComponent<TMP_Text>();
            stands.Add(stand);
        }

        private static void CreateRoom(Transform env, LevelDefinition level)
        {
            var floor = MakeMaterial("Floor", new Color(0.55f, 0.50f, 0.45f));
            var wall = MakeMaterial("Wall", new Color(0.82f, 0.78f, 0.70f));
            var table = MakeMaterial("Table", new Color(0.45f, 0.28f, 0.15f));

            CreateCube("Floor", env, new Vector3(0f, -0.05f, 2f), new Vector3(12f, 0.1f, 12f), floor);
            CreateCube("BackWall", env, new Vector3(0f, 1.5f, 6f), new Vector3(12f, 3f, 0.2f), wall);
            CreateCube("LeftWall", env, new Vector3(-6f, 1.5f, 0f), new Vector3(0.2f, 3f, 12f), wall);
            CreateCube("RightWall", env, new Vector3(6f, 1.5f, 0f), new Vector3(0.2f, 3f, 12f), wall);
            CreateCube("Table", env, new Vector3(0f, 0.375f, 1.6f), new Vector3(1.2f, 0.75f, 0.8f), table);
            CreateCube("Chair", env, new Vector3(-1.6f, 0.25f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), table);
            CreateCube("Chair2", env, new Vector3(1.6f, 0.25f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), table);

            var warm = new GameObject("Warm Light");
            warm.transform.SetParent(env, false);
            warm.transform.localPosition = new Vector3(0f, 2.5f, 2f);
            var point = warm.AddComponent<Light>();
            point.type = LightType.Point;
            point.range = 8f;
            point.intensity = 2f;
            point.color = new Color(1f, 0.85f, 0.65f);
        }

        private static void CreateBazaar(Transform env, LevelDefinition level)
        {
            var floor = MakeMaterial("BazaarFloor", new Color(0.35f, 0.32f, 0.28f));
            var table = MakeMaterial("BazaarTable", new Color(0.45f, 0.28f, 0.15f));
            var canopy = MakeMaterial("BazaarCanopy", new Color(0.75f, 0.18f, 0.15f));
            var wood = MakeMaterial("BazaarWood", new Color(0.45f, 0.28f, 0.15f));
            var lightMat = MakeMaterial("StringLight", new Color(1f, 0.85f, 0.4f), true);

            CreateCube("Floor", env, new Vector3(0f, -0.05f, 0f), new Vector3(16f, 0.1f, 16f), floor);
            CreateCube("CenterTable", env, new Vector3(0f, 0.375f, 0f), new Vector3(1.4f, 0.75f, 1.4f), table);
            CreateCube("CenterTableTop", env, new Vector3(0f, 0.76f, 0f), new Vector3(1.5f, 0.05f, 1.5f), wood);

            var standPositions = new[]
            {
                new Vector3(0f, 0f, 5f),
                new Vector3(5f, 0f, 0f),
                new Vector3(0f, 0f, -5f),
                new Vector3(-5f, 0f, 0f)
            };

            foreach (var pos in standPositions)
            {
                CreateCube("Canopy", env, pos + new Vector3(0f, 2.8f, 0f), new Vector3(2.6f, 0.1f, 2.6f), canopy, false);
                CreateCube("CanopyPost1", env, pos + new Vector3(-1.2f, 1.4f, -1.2f), new Vector3(0.08f, 2.8f, 0.08f), wood, false);
                CreateCube("CanopyPost2", env, pos + new Vector3(1.2f, 1.4f, -1.2f), new Vector3(0.08f, 2.8f, 0.08f), wood, false);
            }

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                var lightGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lightGo.name = "StringLight";
                lightGo.transform.SetParent(env, false);
                lightGo.transform.localPosition = new Vector3(Mathf.Cos(angle) * 4f, 2.6f, Mathf.Sin(angle) * 4f);
                lightGo.transform.localScale = Vector3.one * 0.08f;
                var collider = lightGo.GetComponent<Collider>();
                if (collider != null)
                    UnityEngine.Object.DestroyImmediate(collider);
                lightGo.GetComponent<Renderer>().sharedMaterial = lightMat;
            }


        }

        private static void AddScenesToBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{SceneFolder}/MainVR.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/Level02_Warteg.unity", true),
                new EditorBuildSettingsScene($"{SceneFolder}/Level03_Bazaar.unity", true)
            };
        }

        private static TMP_FontAsset GetFont()
        {
            if (cachedFont != null)
                return cachedFont;
            cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (cachedFont == null)
                cachedFont = TMP_Settings.defaultFontAsset;
            return cachedFont;
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

        private static Vector2 Center => new Vector2(0.5f, 0.5f);
        private static Vector2 Zero => Vector2.zero;
        private static Vector2 One => Vector2.one;
    }
}
