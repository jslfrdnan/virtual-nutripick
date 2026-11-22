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
using NutriPick.Player;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class Scene2WartegBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level2_warteg.unity";
        private const string LevelPath = "Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset";
        private const string OldLevelPath = "Assets/ScriptableObjects/Levels/LD_Level02_Padang.asset";
        private const string MainLevelPath = "Assets/ScriptableObjects/Levels/LD_MainVR.asset";
        private const string RulesPath = "Assets/ScriptableObjects/Rules/NutritionRules.asset";
        private const string PlayerRigPath = "Assets/Prefabs/Player/PlayerRig.prefab";
        private const string FigmaHudPath = "Assets/Prefabs/UI/FigmaHUD.prefab";
        private const string ServingPlatePath = "Assets/Prefabs/Interaction/ServingPlate.prefab";
        private const string ServeStationPath = "Assets/Prefabs/Interaction/ServeStation.prefab";

        private const string WartegStallFbx = "Assets/Models/warteg_stall.fbx";
        private const string DisplayCounterFbx = "Assets/Models/Props/display_counter.fbx";
        private const string TableFbx = "Assets/Models/Props/wooden_table.fbx";
        private const string ChairFbx = "Assets/Models/Props/wooden_chair.fbx";
        private const string BowlFbx = "Assets/Models/Food/Plating/Bowl_Small.fbx";
        private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string FloorMaterialPath = "Assets/Materials/Warteg/M_Warteg_Floor.mat";

        private const float CounterY = 0.17f;
        private const float CounterZ = -0.55f;
        private const float TableTopY = 0.95f;

        private static TMP_FontAsset font;
        private static Material floorMaterial;
        private static TextMeshPro wartegSign;

        [MenuItem("NutriPick/Build Scene 2 Warteg")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            EnsureLevelAsset();
            UpdateMainLevel();
            ConfigureModelImporters();
            EnsureMaterials();
            BuildScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Scene2Warteg] Built Level 2 Warteg with stall, display counter, table, and 7 foods.");
        }

        private static void EnsureLevelAsset()
        {
            if (!File.Exists(LevelPath))
            {
                if (!File.Exists(OldLevelPath))
                {
                    Debug.LogError($"[Scene2Warteg] Missing level asset {OldLevelPath}");
                    return;
                }

                AssetDatabase.MoveAsset(OldLevelPath, LevelPath);
            }

            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            if (level == null)
            {
                Debug.LogError($"[Scene2Warteg] Could not load {LevelPath}");
                return;
            }

            level.levelId = "level_02_warteg";
            level.displayName = "Level 2 — Warteg";
            level.settingDescription = "Makan siang di warteg";
            level.sceneName = "Level2_warteg";
            level.nextSceneName = "Level03_Bazaar";
            level.levelIndex = 2;
            level.minItems = 3;
            level.requiresDrink = false;
            level.minimumStands = 1;
            level.timeLimitSeconds = 180f;
            level.graceSeconds = 30f;
            level.isFinalLevel = false;
            level.learningFocus = new[] { "porsi", "lemak", "natrium" };
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
        }

        private static void UpdateMainLevel()
        {
            var main = AssetDatabase.LoadAssetAtPath<LevelDefinition>(MainLevelPath);
            if (main == null)
            {
                Debug.LogError($"[Scene2Warteg] Missing {MainLevelPath}");
                return;
            }

            main.nextSceneName = "Level2_warteg";
            EditorUtility.SetDirty(main);
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureModelImporters()
        {
            var paths = new[] { WartegStallFbx, DisplayCounterFbx, TableFbx, ChairFbx };
            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"[Scene2Warteg] Missing model: {path}");
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

            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        }

        private static void EnsureMaterials()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials/Warteg"))
                AssetDatabase.CreateFolder("Assets/Materials", "Warteg");

            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null)
            {
                Debug.LogError("[Scene2Warteg] URP Lit shader not found.");
                return;
            }

            floorMaterial = AssetDatabase.LoadAssetAtPath<Material>(FloorMaterialPath);
            if (floorMaterial == null)
            {
                floorMaterial = new Material(lit);
                AssetDatabase.CreateAsset(floorMaterial, FloorMaterialPath);
            }

            floorMaterial.shader = lit;
            floorMaterial.SetColor("_BaseColor", new Color(0.52f, 0.49f, 0.45f));
            floorMaterial.SetFloat("_Metallic", 0f);
            floorMaterial.SetFloat("_Smoothness", 0.12f);
            EditorUtility.SetDirty(floorMaterial);
            AssetDatabase.SaveAssets();
        }

        private static void BuildScene()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            var rules = AssetDatabase.LoadAssetAtPath<NutritionRules>(RulesPath);
            if (level == null || rules == null || level.foodPool == null || level.foodPool.Length != 7)
            {
                Debug.LogError("[Scene2Warteg] Missing level, rules, or seven-food pool.");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Scene2Warteg");

            CreateExterior(root.transform);
            CreateStall(root.transform);
            var stand = CreateDisplayCounter(root.transform, level);
            CreateTableSet(root.transform);
            var plate = CreateServingPlate(root.transform);
            var serveStation = CreateServeStation(root.transform);
            var player = CreatePlayer();
            var hud = CreateHud();
            CreateFlow(root.transform, level, rules, stand, plate, serveStation, player, hud);
            ConfigureLighting(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateExterior(Transform root)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground_Warteg";
            ground.transform.SetParent(root, false);
            ground.transform.localPosition = new Vector3(0f, -0.06f, 0.5f);
            ground.transform.localScale = new Vector3(10f, 0.12f, 8f);
            if (floorMaterial != null)
                ground.GetComponent<Renderer>().sharedMaterial = floorMaterial;
        }

        private static void CreateStall(Transform root)
        {
            var stall = InstantiateModel(WartegStallFbx, root, "WartegStall", Vector3.zero, Quaternion.identity);
            if (stall == null)
                return;

            StripCamerasAndLights(stall);
            AddBox(stall.transform, "Collider_Floor", new Vector3(0f, 0.08f, 0f), new Vector3(4.5f, 0.16f, 3.0f));
            AddBox(stall.transform, "Collider_BackWall", new Vector3(0f, 1.3f, -1.5f), new Vector3(4.5f, 2.6f, 0.15f));
            AddBox(stall.transform, "Collider_SideL", new Vector3(-2.2f, 1.3f, 0f), new Vector3(0.15f, 2.6f, 3.0f));
            AddBox(stall.transform, "Collider_SideR", new Vector3(2.2f, 1.3f, 0f), new Vector3(0.15f, 2.6f, 3.0f));

            CreateWorldText(root, "Sign_WartegSub", "NASI · LAUK · SAYUR",
                new Vector3(0f, 2.82f, 1.57f), Quaternion.identity, 0.052f, 36f,
                new Color(0.22f, 0.11f, 0.07f), TextAlignmentOptions.Center);
            wartegSign = CreateWorldText(root, "Sign_Warteg", "WARTEG",
                new Vector3(0f, 3.02f, 1.57f), Quaternion.identity, 0.13f, 36f,
                new Color(0.35f, 0.06f, 0.04f), TextAlignmentOptions.Center);
        }

        private static FoodStand CreateDisplayCounter(Transform root, LevelDefinition level)
        {
            var counter = InstantiateModel(DisplayCounterFbx, root, "DisplayCounter",
                new Vector3(0f, CounterY, CounterZ), Quaternion.identity);
            if (counter == null)
                return null;

            StripCamerasAndLights(counter);
            AddBox(counter.transform, "Collider_Base", new Vector3(0f, 0.08f, 0f), new Vector3(2.5f, 0.16f, 0.82f));

            var standObject = new GameObject("WartegFoodStand");
            standObject.transform.SetParent(counter.transform, false);
            standObject.transform.localPosition = Vector3.zero;

            var stand = standObject.AddComponent<FoodStand>();
            stand.displayName = "Warteg";
            stand.foods = level.foodPool;
            stand.anchors = new Transform[level.foodPool.Length];

            var positions = new[]
            {
                new Vector3(0.99f, 0.22f, 0.02f),
                new Vector3(0.60f, 0.22f, 0.02f),
                new Vector3(0.21f, 0.22f, 0.02f),
                new Vector3(-0.21f, 0.22f, 0.02f),
                new Vector3(-0.60f, 0.22f, 0.02f),
                new Vector3(-0.99f, 0.22f, 0.02f),
                new Vector3(0.80f, 0.61f, 0.00f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var anchor = new GameObject($"FoodAnchor_{i + 1}");
                anchor.transform.SetParent(standObject.transform, false);
                anchor.transform.localPosition = positions[i];
                stand.anchors[i] = anchor.transform;
            }

            var bowl = InstantiateModel(BowlFbx, standObject.transform, "Bowl_Shelf",
                new Vector3(0.80f, 0.575f, 0f), Quaternion.identity);
            if (bowl != null)
                bowl.transform.localScale = Vector3.one * 0.7f;

            var sign = CreateWorldText(root, "Sign_WartegStand", "WARTEG",
                new Vector3(0f, 3.02f, 1.58f), Quaternion.identity, 0.13f, 36f,
                new Color(0.35f, 0.06f, 0.04f), TextAlignmentOptions.Center);
            stand.signText = sign;
            return stand;
        }

        private static void CreateTableSet(Transform root)
        {
            var table = InstantiateModel(TableFbx, root, "WoodenTable",
                new Vector3(1.2f, 0.17f, 0.9f), Quaternion.identity);
            if (table != null)
                AddBox(table.transform, "Collider_Table", new Vector3(0f, 0.39f, 0f), new Vector3(1.6f, 0.78f, 0.75f));

            var chairA = InstantiateModel(ChairFbx, root, "WoodenChair_A",
                new Vector3(1.2f, 0.17f, 1.6f), Quaternion.Euler(0f, 180f, 0f));
            if (chairA != null)
                AddBox(chairA.transform, "Collider_Chair", new Vector3(0f, 0.5f, 0f), new Vector3(0.46f, 1.0f, 0.54f));

            var chairB = InstantiateModel(ChairFbx, root, "WoodenChair_B",
                new Vector3(1.2f, 0.17f, 0.2f), Quaternion.identity);
            if (chairB != null)
                AddBox(chairB.transform, "Collider_Chair", new Vector3(0f, 0.5f, 0f), new Vector3(0.46f, 1.0f, 0.54f));
        }

        private static PlateController CreateServingPlate(Transform root)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServingPlatePath);
            if (prefab == null)
            {
                Debug.LogError($"[Scene2Warteg] Missing {ServingPlatePath}");
                return null;
            }

            var plateObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            plateObject.name = "ServingPlate";
            plateObject.transform.localPosition = new Vector3(1.2f, TableTopY + 0.01f, 0.9f);
            plateObject.transform.localRotation = Quaternion.identity;
            return plateObject.GetComponent<PlateController>();
        }

        private static ServeStation CreateServeStation(Transform root)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServeStationPath);
            if (prefab == null)
            {
                Debug.LogError($"[Scene2Warteg] Missing {ServeStationPath}");
                return null;
            }

            var stationObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            stationObject.name = "ServeStation";
            stationObject.transform.localPosition = new Vector3(1.7f, TableTopY + 0.01f, 0.9f);
            stationObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            return stationObject.GetComponent<ServeStation>();
        }

        private static FirstPersonController CreatePlayer()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerRigPath);
            if (prefab == null)
            {
                Debug.LogError($"[Scene2Warteg] Missing {PlayerRigPath}");
                return null;
            }

            var playerObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            playerObject.name = "PlayerRig";
            playerObject.transform.position = new Vector3(0f, 0.2f, 1.8f);
            playerObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            return playerObject.GetComponent<FirstPersonController>();
        }

        private static GameObject CreateHud()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FigmaHudPath);
            if (prefab == null)
            {
                Debug.LogError($"[Scene2Warteg] Missing {FigmaHudPath}");
                return null;
            }

            var hud = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            hud.name = "FigmaHUD";
            return hud;
        }

        private static void CreateFlow(
            Transform root,
            LevelDefinition level,
            NutritionRules rules,
            FoodStand stand,
            PlateController plate,
            ServeStation serveStation,
            FirstPersonController player,
            GameObject hud)
        {
            var playerObject = player != null ? player.gameObject : null;
            var reticle = playerObject != null ? playerObject.GetComponent<ReticleInteractor>() : null;

            var flowObject = new GameObject("LevelFlow");
            flowObject.transform.SetParent(root, false);
            var flow = flowObject.AddComponent<LevelFlowController>();
            flow.level = level;
            flow.rules = rules;
            flow.plate = plate;
            flow.stands = stand != null ? new[] { stand } : new FoodStand[0];
            flow.serveStation = serveStation;
            flow.player = player;
            flow.reticle = reticle;

            if (hud != null)
            {
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
            }

            if (reticle != null)
            {
                reticle.flow = flow;
                reticle.plate = plate;
                reticle.serveStation = serveStation;
                reticle.infoCard = flow.infoCard;
                reticle.foodCards = flow.foodCards;
                reticle.prompt = flow.prompt;
            }
        }

        private static void ConfigureLighting(Transform root)
        {
            var sunObject = new GameObject("Sun");
            sunObject.transform.SetParent(root, false);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
            sun.color = new Color(1f, 0.88f, 0.72f);
            sun.intensity = 1.0f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.65f;
            RenderSettings.sun = sun;

            var lighting = new GameObject("Lighting");
            lighting.transform.SetParent(root, false);
            AddPointLight(lighting.transform, "Lamp_Counter", new Vector3(0f, 2.3f, CounterZ), 5f, 1.1f);
            AddPointLight(lighting.transform, "Lamp_Table", new Vector3(1.2f, 2.3f, 0.9f), 4f, 0.7f);
            AddPointLight(lighting.transform, "Lamp_Front", new Vector3(0f, 2.5f, 2.4f), 5f, 0.45f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.38f, 0.34f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = false;
            RenderSettings.skybox = null;
            DynamicGI.UpdateEnvironment();
        }

        private static void AddPointLight(Transform parent, string name, Vector3 position, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.color = new Color(1f, 0.80f, 0.56f);
            light.shadows = LightShadows.None;
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
            return wrapper;
        }

        private static void StripCamerasAndLights(GameObject root)
        {
            foreach (var camera in root.GetComponentsInChildren<Camera>(true))
            {
                if (camera != null)
                    Object.DestroyImmediate(camera.gameObject);
            }

            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (light != null)
                    Object.DestroyImmediate(light.gameObject);
            }

            foreach (var listener in root.GetComponentsInChildren<AudioListener>(true))
            {
                if (listener != null)
                    Object.DestroyImmediate(listener.gameObject);
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
                scenes.Insert(mainIndex >= 0 ? mainIndex + 1 : scenes.Count,
                    new EditorBuildSettingsScene(ScenePath, true));
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
