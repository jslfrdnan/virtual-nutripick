using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.Player;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class Scene1FigmaBuilder
    {
        private const string ScenePath = "Assets/Scenes/MainVR.unity";
        private const string ServingPlatePath = "Assets/Prefabs/Interaction/ServingPlate.prefab";
        private const string FigmaHudPath = "Assets/Prefabs/UI/FigmaHUD.prefab";

        [MenuItem("NutriPick/Build Scene 1 Figma Setup")]
        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_MainVR.asset");
            if (level == null)
            {
                Debug.LogError("[Scene1Figma] Missing LD_MainVR.asset");
                return;
            }

            var expected = new Dictionary<string, Vector3>
            {
                { "rendang", new Vector3(-0.70f, 1.36f, 0.05f) },
                { "burger", new Vector3(-0.50f, 1.36f, 0.05f) },
                { "telur_rebus", new Vector3(-0.30f, 1.36f, 0.05f) },
                { "telur_ceplok", new Vector3(-0.10f, 1.36f, 0.05f) },
                { "tempe", new Vector3(0.10f, 1.36f, 0.05f) },
                { "brokoli", new Vector3(0.30f, 1.36f, 0.05f) },
                { "pisang", new Vector3(0.50f, 1.36f, 0.05f) },
                { "mie_goreng", new Vector3(0.70f, 1.36f, 0.05f) }
            };

            var kept = new Dictionary<string, FoodItem>();
            foreach (var item in Object.FindObjectsByType<FoodItem>(FindObjectsSortMode.None))
            {
                if (item == null || item.data == null || !expected.ContainsKey(item.data.id))
                    continue;

                if (kept.ContainsKey(item.data.id))
                {
                    Object.DestroyImmediate(item.gameObject);
                    continue;
                }

                kept[item.data.id] = item;
            }

            foreach (var pair in expected)
            {
                if (!kept.TryGetValue(pair.Key, out var item))
                {
                    Debug.LogError($"[Scene1Figma] Missing food item {pair.Key}");
                    continue;
                }

                item.transform.SetParent(null, true);
                item.transform.position = pair.Value;
                item.transform.rotation = Quaternion.identity;
            }

            var stand = Object.FindFirstObjectByType<FoodStand>(FindObjectsInactive.Include);
            if (stand == null)
            {
                var standObject = new GameObject("TableFoodStand");
                stand = standObject.AddComponent<FoodStand>();
            }
            stand.displayName = "Meja Warteg";
            stand.foods = level.foodPool;
            stand.anchors = new Transform[level.foodPool.Length];
            for (int i = 0; i < level.foodPool.Length; i++)
            {
                var food = level.foodPool[i];
                if (food != null && kept.TryGetValue(food.id, out var item))
                    stand.anchors[i] = item.transform;
            }
            stand.gameObject.SetActive(false);

            var oldPlate = Object.FindFirstObjectByType<PlateController>();
            var oldPlateObject = oldPlate != null ? oldPlate.gameObject : GameObject.Find("Plate");
            var platePosition = oldPlateObject != null
                ? oldPlateObject.transform.position
                : new Vector3(0f, 1.34f, -0.55f);

            var platePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ServingPlatePath);
            if (platePrefab == null)
            {
                Debug.LogError($"[Scene1Figma] Missing {ServingPlatePath}");
                return;
            }

            var newPlateObject = (GameObject)PrefabUtility.InstantiatePrefab(platePrefab);
            newPlateObject.name = "Plate";
            newPlateObject.transform.position = platePosition;
            newPlateObject.transform.rotation = Quaternion.identity;
            var plate = newPlateObject.GetComponent<PlateController>();
            if (oldPlateObject != null)
                Object.DestroyImmediate(oldPlateObject);

            var oldHud = GameObject.Find("HUD");
            if (oldHud == null)
                oldHud = GameObject.Find("FigmaHUD");
            var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FigmaHudPath);
            if (hudPrefab == null)
            {
                Debug.LogError($"[Scene1Figma] Missing {FigmaHudPath}");
                return;
            }

            var newHud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            newHud.name = "FigmaHUD";
            if (oldHud != null)
                Object.DestroyImmediate(oldHud);

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            var reticle = Object.FindFirstObjectByType<ReticleInteractor>();
            var serveStation = Object.FindFirstObjectByType<ServeStation>();
            if (flow == null || reticle == null)
            {
                Debug.LogError("[Scene1Figma] Missing LevelFlowController or ReticleInteractor");
                return;
            }

            flow.level = level;
            flow.plate = plate;
            flow.stands = new[] { stand };
            flow.serveStation = serveStation;
            flow.startScreen = newHud.GetComponentInChildren<StartScreenPresenter>(true);
            flow.foodCards = newHud.GetComponentInChildren<FoodCardGridPresenter>(true);
            flow.mealStatus = newHud.GetComponentInChildren<MealStatusBarPresenter>(true);
            flow.bar = newHud.GetComponentInChildren<NutritionBar>(true);
            flow.timer = newHud.GetComponentInChildren<LevelTimerDisplay>(true);
            flow.prompt = newHud.GetComponentInChildren<InteractionPrompt>(true);
            flow.infoCard = newHud.GetComponentInChildren<FoodInfoCardPresenter>(true);
            flow.report = newHud.GetComponentInChildren<HealthReportPresenter>(true);
            flow.finalResult = newHud.GetComponentInChildren<FinalResultPresenter>(true);
            flow.recap = newHud.GetComponentInChildren<RecapPresenter>(true);
            flow.pausePanel = FindDeepChild(newHud.transform, "PausePanel")?.gameObject;
            flow.pauseMessage = FindDeepChild(newHud.transform, "PauseMessage")?.GetComponent<TMPro.TMP_Text>();
            flow.pauseResumeButton = FindDeepChild(newHud.transform, "ResumeButton")?.GetComponent<UnityEngine.UI.Button>();
            flow.pauseQuitButton = FindDeepChild(newHud.transform, "QuitButton")?.GetComponent<UnityEngine.UI.Button>();

            reticle.flow = flow;
            reticle.plate = plate;
            reticle.serveStation = serveStation;
            reticle.infoCard = flow.infoCard;
            reticle.foodCards = flow.foodCards;
            reticle.prompt = flow.prompt;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainVR.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level02_Padang.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level03_Bazaar.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Scene1Figma] Scene 1 wired with ServingPlate, FigmaHUD, StartScreen, and 8 foods.");
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
