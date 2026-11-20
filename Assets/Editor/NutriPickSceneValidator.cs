using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class NutriPickSceneValidator
    {
        private static readonly string[] SceneCandidates =
        {
            "Assets/Scenes/MainVR.unity",
            "Assets/Scenes/Level02_Padang.unity",
            "Assets/Scenes/Level03_Bazaar.unity"
        };

        private static string[] ExistingScenes()
        {
            return SceneCandidates.Where(File.Exists).ToArray();
        }

        [MenuItem("NutriPick/Validate Scenes")]
        public static void ValidateScenes()
        {
            int errors = 0;
            foreach (var path in ExistingScenes())
            {
                var scene = EditorSceneManager.OpenScene(path);
                var flow = Object.FindFirstObjectByType<LevelFlowController>();
                if (flow == null)
                {
                    errors++;
                    Debug.LogError($"[NutriPick] {path}: missing LevelFlowController");
                    continue;
                }

                if (flow.level == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing LevelDefinition"); }
                if (flow.rules == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing NutritionRules"); }
                if (flow.plate == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing PlateController"); }
                if (flow.plate != null && flow.plate.slots != null && flow.plate.slots.Length != 4) { errors++; Debug.LogError($"[NutriPick] {path}: plate has {flow.plate.slots.Length} slots"); }
                if (flow.stands == null || flow.stands.Length == 0) { errors++; Debug.LogError($"[NutriPick] {path}: no FoodStand"); }
                if (flow.serveStation == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing ServeStation"); }
                if (flow.player == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FirstPersonController"); }
                if (flow.reticle == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing ReticleInteractor"); }
                if (flow.bar == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing NutritionBar"); }
                if (flow.timer == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing LevelTimerDisplay"); }
                if (flow.prompt == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing InteractionPrompt"); }
                if (flow.infoCard == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FoodInfoCardPresenter"); }
                if (flow.report == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing HealthReportPresenter"); }
                if (flow.recap == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing RecapPresenter"); }
                if (flow.pausePanel == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing PausePanel"); }

                if (path.Contains("MainVR"))
                {
                    if (flow.foodCards == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FoodCardGridPresenter"); }
                    if (flow.mealStatus == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing MealStatusBarPresenter"); }
                    if (flow.finalResult == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FinalResultPresenter"); }

                    var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (eventSystems.Length != 1) { errors++; Debug.LogError($"[NutriPick] {path}: expected 1 EventSystem, found {eventSystems.Length}"); }

                    var plates = Object.FindObjectsByType<PlateController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (plates.Length != 1) { errors++; Debug.LogError($"[NutriPick] {path}: expected 1 serving plate, found {plates.Length}"); }

                    var grids = Object.FindObjectsByType<FoodCardGridPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (grids.Length != 1) { errors++; Debug.LogError($"[NutriPick] {path}: expected 1 FoodCardGridPresenter, found {grids.Length}"); }

                    var finalResults = Object.FindObjectsByType<FinalResultPresenter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (finalResults.Length != 1) { errors++; Debug.LogError($"[NutriPick] {path}: expected 1 FinalResultPresenter, found {finalResults.Length}"); }

                    var foods = Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (foods.Length != 8) { errors++; Debug.LogError($"[NutriPick] {path}: expected 8 food items, found {foods.Length}"); }

                    if (flow.stands == null || flow.stands.Length != 8) { errors++; Debug.LogError($"[NutriPick] {path}: expected 8 food plates, found {flow.stands?.Length ?? 0}"); }
                    if (flow.stands != null && flow.stands.Any(stand => stand == null || stand.foods == null || stand.foods.Length != 1)) { errors++; Debug.LogError($"[NutriPick] {path}: every food plate must hold exactly one food"); }
                    if (GameObject.Find("Scene1Home") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing Scene1Home environment"); }

                    foreach (var tableObject in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                    {
                        if (tableObject == null)
                            continue;
                        if (tableObject.name == "TableVase"
                            || tableObject.name.StartsWith("TablePlant")
                            || tableObject.name.StartsWith("Tray_")
                            || tableObject.name == "LaukPlatter"
                            || tableObject.name == "KarboSayurPlatter")
                        {
                            errors++;
                            Debug.LogError($"[NutriPick] {path}: non-food table object still active: {tableObject.name}");
                        }
                    }
                }

                Debug.Log($"[NutriPick] {path}: flow OK, stands={flow.stands?.Length}, plateSlots={flow.plate?.slots?.Length}");
            }

            foreach (var scene in EditorBuildSettings.scenes)
                Debug.Log($"[NutriPick] Build scene: {scene.path} enabled={scene.enabled}");

            Debug.Log(errors == 0 ? "[NutriPick] Scene validation passed." : $"[NutriPick] Scene validation found {errors} error(s).");
        }

        [MenuItem("NutriPick/Validate And Capture")]
        public static void ValidateAndCapture()
        {
            ValidateScenes();
            CaptureScenePreviews();
        }

        [MenuItem("NutriPick/Capture Scene Previews")]
        public static void CaptureScenePreviews()
        {
            var output = Path.Combine(Path.GetTempPath(), "opencode");
            Directory.CreateDirectory(output);

            foreach (var path in ExistingScenes())
            {
                EditorSceneManager.OpenScene(path);
                var camera = Camera.main;
                if (camera == null)
                {
                    Debug.LogError($"[NutriPick] {path}: no main camera");
                    continue;
                }

                if (path.Contains("MainVR"))
                {
                    Capture(camera, output, "MainVR_FirstPerson", new Vector3(0.4f, 1.6f, -2f), Quaternion.Euler(8f, -20f, 0f));
                    Capture(camera, output, "MainVR_Buffet", new Vector3(0.4f, 1.5f, -1.2f), Quaternion.Euler(15f, -30f, 0f));

                    var wasOrthographic = camera.orthographic;
                    var previousSize = camera.orthographicSize;
                    camera.orthographic = true;
                    camera.orthographicSize = 0.65f;
                    Capture(camera, output, "MainVR_PlattersTop", new Vector3(-1.0f, 2.4f, -1.1f), Quaternion.Euler(90f, 0f, 0f));
                    camera.orthographic = wasOrthographic;
                    camera.orthographicSize = previousSize;

                    var ceiling = FindSceneObject("Ceiling");
                    var ceilingActive = ceiling != null && ceiling.activeSelf;
                    if (ceiling != null)
                        ceiling.SetActive(false);
                    Capture(camera, output, "MainVR_Top", new Vector3(0f, 4f, 0f), Quaternion.Euler(90f, 0f, 0f));
                    if (ceiling != null)
                        ceiling.SetActive(ceilingActive);
                }
                else
                {
                    Capture(camera, output, Path.GetFileNameWithoutExtension(path), camera.transform.position, camera.transform.rotation);
                }
            }
        }

        private static GameObject FindSceneObject(string name)
        {
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (transform != null && transform.name == name)
                    return transform.gameObject;
            }

            return null;
        }

        private static void Capture(Camera camera, string output, string name, Vector3 position, Quaternion rotation)
        {
            camera.transform.position = position;
            camera.transform.rotation = rotation;

            var rt = new RenderTexture(1280, 720, 24);
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;

            var file = Path.Combine(output, name + ".png");
            File.WriteAllBytes(file, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            rt.Release();
            Object.DestroyImmediate(rt);
            Debug.Log($"[NutriPick] Captured {file}");
        }
    }
}
