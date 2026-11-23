using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.NPC;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class NutriPickSceneValidator
    {
        private static readonly string[] SceneCandidates =
        {
            "Assets/Scenes/MainVR.unity",
            "Assets/Scenes/Level02_Warteg.unity",
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
                if (flow.prompt == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing InteractionPrompt"); }
                if (flow.infoCard == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FoodInfoCardPresenter"); }
                if (flow.report == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing HealthReportPresenter"); }
                if (flow.pausePanel == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing PausePanel"); }

                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    bool offsetCamera = mainCamera.transform.localPosition.sqrMagnitude > 0.0001f
                        || Quaternion.Angle(mainCamera.transform.localRotation, Quaternion.identity) > 0.1f
                        || (mainCamera.transform.localScale - Vector3.one).sqrMagnitude > 0.0001f;
                    if (offsetCamera)
                    {
                        errors++;
                        Debug.LogError($"[NutriPick] {path}: main camera is offset from the player rig (localPos={mainCamera.transform.localPosition}, localRot={mainCamera.transform.localEulerAngles})");
                    }
                }

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
                    var foodTypes = foods.Where(food => food.data != null).Select(food => food.data.id).Distinct().Count();
                    if (foodTypes != 8) { errors++; Debug.LogError($"[NutriPick] {path}: expected 8 food types, found {foodTypes}"); }
                    if (foods.Length < 8) { errors++; Debug.LogError($"[NutriPick] {path}: expected at least 8 food items, found {foods.Length}"); }

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
                else if (path.Contains("Level02_Warteg"))
                {
                    if (flow.foodCards == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FoodCardGridPresenter"); }
                    if (flow.mealStatus == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing MealStatusBarPresenter"); }
                    if (flow.finalResult == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FinalResultPresenter"); }

                    var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (eventSystems.Length != 1) { errors++; Debug.LogError($"[NutriPick] {path}: expected 1 EventSystem, found {eventSystems.Length}"); }

                    var foods = Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (foods.Length != 21) { errors++; Debug.LogError($"[NutriPick] {path}: expected 21 food items (3 portions x 7 dishes), found {foods.Length}"); }

                    if (flow.stands == null || flow.stands.Length != 1) { errors++; Debug.LogError($"[NutriPick] {path}: expected 1 food display, found {flow.stands?.Length ?? 0}"); }
                    if (flow.stands != null && flow.stands.Any(stand => stand == null || stand.foods == null || stand.foods.Length != 7)) { errors++; Debug.LogError($"[NutriPick] {path}: the Warteg display must hold seven foods"); }
                    if (GameObject.Find("Ruko_West_1") == null || GameObject.Find("Ruko_East_1") == null || GameObject.Find("Building_North_1") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing the new shophouse world"); }

                    if (GameObject.Find("Scene2Warteg") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing Scene2Warteg environment"); }
                    if (GameObject.Find("WartegTegalSari") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing WartegTegalSari"); }
                    if (flow.player != null && flow.player.transform.position.z < 6f) { errors++; Debug.LogError($"[NutriPick] {path}: player does not spawn on the Warteg sidewalk"); }

                    errors += ValidateNpcCast(path, 5);
                }
                else if (path.Contains("Level03_Bazaar"))
                {
                    if (flow.foodCards == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FoodCardGridPresenter"); }
                    if (flow.mealStatus == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing MealStatusBarPresenter"); }
                    if (flow.finalResult == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing FinalResultPresenter"); }

                    var eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (eventSystems.Length != 1) { errors++; Debug.LogError($"[NutriPick] {path}: expected 1 EventSystem, found {eventSystems.Length}"); }

                    var foods = Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (foods.Length != 120) { errors++; Debug.LogError($"[NutriPick] {path}: expected 120 food items, found {foods.Length}"); }

                    if (flow.stands == null || flow.stands.Length != 30) { errors++; Debug.LogError($"[NutriPick] {path}: expected 30 bazaar stands, found {flow.stands?.Length ?? 0}"); }
                    if (flow.stands != null && flow.stands.Any(stand => stand == null || stand.foods == null || stand.foods.Length != 2)) { errors++; Debug.LogError($"[NutriPick] {path}: every bazaar stand must hold exactly two food types"); }

                    var decorStalls = Object.FindObjectsByType<DecorStall>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (decorStalls.Length != 0) { errors++; Debug.LogError($"[NutriPick] {path}: expected no closed decorative stalls, found {decorStalls.Length}"); }

                    if (GameObject.Find("Scene3Bazaar") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing Scene3Bazaar environment"); }
                    if (GameObject.Find("SchoolCampus") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing SchoolCampus backdrop"); }
                    if (GameObject.Find("Cafe_Kopi") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing Cafe_Kopi"); }
                    if (GameObject.Find("Cafe_Bakar") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing Cafe_Bakar"); }
                    if (GameObject.Find("PanggungBazaar") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing PanggungBazaar"); }
                    if (GameObject.Find("Road_Flamboyant_Cross") == null || GameObject.Find("Intersection_Plaza") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing the new Flamboyant intersection"); }
                    if (GameObject.Find("CoffeeShop_01") == null || GameObject.Find("CoffeeShop_04") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing coffee shop backdrop"); }
                    if (GameObject.Find("SMK_Building") == null) { errors++; Debug.LogError($"[NutriPick] {path}: missing SMK backdrop"); }

                    if (flow.serveStation != null && flow.serveStation.transform.position.x > 2f) { errors++; Debug.LogError($"[NutriPick] {path}: serve station is inside the school campus"); }
                    if (flow.player != null && (flow.player.transform.position.x > -6f || flow.player.transform.position.z < 4.5f)) { errors++; Debug.LogError($"[NutriPick] {path}: player does not spawn in front of the serving plate"); }

                    errors += ValidateNpcCast(path, 9);
                }

                Debug.Log($"[NutriPick] {path}: flow OK, stands={flow.stands?.Length}, plateSlots={flow.plate?.slots?.Length}");
            }

            foreach (var scene in EditorBuildSettings.scenes)
                Debug.Log($"[NutriPick] Build scene: {scene.path} enabled={scene.enabled}");

            Debug.Log(errors == 0 ? "[NutriPick] Scene validation passed." : $"[NutriPick] Scene validation found {errors} error(s).");
        }

        private static int ValidateNpcCast(string path, int expected)
        {
            int errors = 0;
            var npcLayer = LayerMask.NameToLayer("NPC");
            if (npcLayer < 0)
            {
                Debug.LogError("[NutriPick] NPC layer is missing.");
                return 1;
            }

            var markers = Object.FindObjectsByType<NpcMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (markers.Length != expected)
            {
                errors++;
                Debug.LogError($"[NutriPick] {path}: expected {expected} NPCs, found {markers.Length}");
            }

            foreach (var marker in markers)
            {
                if (marker.gameObject.layer != npcLayer)
                {
                    errors++;
                    Debug.LogError($"[NutriPick] {path}: {marker.name} is not on the NPC layer");
                }

                if (marker.GetComponent<Animator>() == null || marker.GetComponent<NpcAnimator>() == null)
                {
                    errors++;
                    Debug.LogError($"[NutriPick] {path}: {marker.name} is missing its animator setup");
                }

                if (marker.GetComponent<Collider>() == null)
                {
                    errors++;
                    Debug.LogError($"[NutriPick] {path}: {marker.name} has no collider");
                }
            }

            foreach (var pathAsset in new[]
                     {
                         "Assets/ScriptableObjects/Levels/LD_MainVR.asset",
                         "Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset",
                         "Assets/ScriptableObjects/Levels/LD_Level03_Bazaar.asset"
                     })
            {
                if (AssetDatabase.LoadAssetAtPath<LevelDefinition>(pathAsset) == null)
                {
                    errors++;
                    Debug.LogError($"[NutriPick] Missing level asset {pathAsset}");
                }
            }

            var level1 = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_MainVR.asset");
            var level2 = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset");
            if (level1 != null && level1.nextSceneName != "Level02_Warteg")
            {
                errors++;
                Debug.LogError("[NutriPick] LD_MainVR must continue to Level02_Warteg.");
            }
            if (level2 != null && level2.nextSceneName != "Level03_Bazaar")
            {
                errors++;
                Debug.LogError("[NutriPick] LD_Level02_Warteg must continue to Level03_Bazaar.");
            }

            return errors;
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
                else if (path.Contains("Level02_Warteg"))
                {
                    Capture(camera, output, "Level02_Spawn", new Vector3(0f, 1.65f, 8.2f), Quaternion.Euler(3f, 180f, 0f));
                    Capture(camera, output, "Level02_Door", new Vector3(0f, 1.65f, 4.8f), Quaternion.Euler(4f, 180f, 0f));
                    Capture(camera, output, "Level02_Counter", new Vector3(0.2f, 1.62f, 0.3f), Quaternion.Euler(4f, 180f, 0f));
                    Capture(camera, output, "Level02_Dining", new Vector3(0.8f, 1.62f, 2.9f), Quaternion.Euler(4f, -112f, 0f));
                    Capture(camera, output, "Level02_Cashier", new Vector3(-0.6f, 1.6f, 2.9f), Quaternion.Euler(4f, 111f, 0f));

                    var shell = FindSceneObject("WartegTegalSari");
                    var roof = shell != null ? FindDeepChild(shell.transform, "Roof") : null;
                    var awning = shell != null ? FindDeepChild(shell.transform, "Awning") : null;
                    var ceiling = shell != null ? FindDeepChild(shell.transform, "Ceiling") : null;
                    var roofActive = roof != null && roof.gameObject.activeSelf;
                    var awningActive = awning != null && awning.gameObject.activeSelf;
                    var ceilingActive = ceiling != null && ceiling.gameObject.activeSelf;
                    if (roof != null) roof.gameObject.SetActive(false);
                    if (awning != null) awning.gameObject.SetActive(false);
                    if (ceiling != null) ceiling.gameObject.SetActive(false);

                    var wasOrthographic = camera.orthographic;
                    var previousSize = camera.orthographicSize;
                    camera.orthographic = true;
                    camera.orthographicSize = 12f;
                    Capture(camera, output, "Level02_Top", new Vector3(0f, 25f, 4f), Quaternion.Euler(90f, 0f, 0f));
                    camera.orthographic = wasOrthographic;
                    camera.orthographicSize = previousSize;
                    if (roof != null) roof.gameObject.SetActive(roofActive);
                    if (awning != null) awning.gameObject.SetActive(awningActive);
                    if (ceiling != null) ceiling.gameObject.SetActive(ceilingActive);
                }
                else if (path.Contains("Level03_Bazaar"))
                {
                    Capture(camera, output, "Level03_Spawn", new Vector3(-9f, 1.65f, 41.5f), Quaternion.Euler(4f, 180f, 0f));
                    Capture(camera, output, "Level03_WestRow", new Vector3(-7f, 1.7f, 18f), Quaternion.Euler(4f, -90f, 0f));
                    Capture(camera, output, "Level03_EastRow", new Vector3(-11f, 1.7f, 13f), Quaternion.Euler(4f, 90f, 0f));
                    Capture(camera, output, "Level03_Stage", new Vector3(-8f, 2.8f, 15f), Quaternion.Euler(8f, 180f, 0f));
                    Capture(camera, output, "Level03_School", new Vector3(-8f, 2f, 8f), Quaternion.Euler(5f, 90f, 0f));
                    Capture(camera, output, "Level03_Pramuka", new Vector3(-9f, 1.65f, -31f), Quaternion.Euler(4f, 95f, 0f));

                    var wasOrthographic = camera.orthographic;
                    var previousSize = camera.orthographicSize;
                    camera.orthographic = true;
                    camera.orthographicSize = 38f;
                    Capture(camera, output, "Level03_Top", new Vector3(-9f, 60f, -2f), Quaternion.Euler(90f, 0f, 0f));
                    camera.orthographic = wasOrthographic;
                    camera.orthographicSize = previousSize;
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

        private static void Capture(Camera camera, string output, string name, Vector3 position, Quaternion rotation)
        {
            var previousPosition = camera.transform.position;
            var previousRotation = camera.transform.rotation;
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;

            var rt = new RenderTexture(1280, 720, 24);
            try
            {
                camera.transform.position = position;
                camera.transform.rotation = rotation;

                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;

                var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                try
                {
                    tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                    tex.Apply();

                    var file = Path.Combine(output, name + ".png");
                    File.WriteAllBytes(file, tex.EncodeToPNG());
                    Debug.Log($"[NutriPick] Captured {file}");
                }
                finally
                {
                    Object.DestroyImmediate(tex);
                }
            }
            finally
            {
                RenderTexture.active = previousActive;
                camera.targetTexture = previousTarget;
                camera.transform.position = previousPosition;
                camera.transform.rotation = previousRotation;
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }
    }
}
