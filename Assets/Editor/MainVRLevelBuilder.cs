using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.Nutrition;
using NutriPick.Player;
using NutriPick.UI;

namespace NutriPick.Editor
{
    public static class MainVRLevelBuilder
    {
        [MenuItem("NutriPick/Build MainVR Level 1")]
        public static void BuildMainVR()
        {
            if (GameObject.Find("LevelFlow") != null)
            {
                Debug.LogWarning("[NutriPick] MainVR is already wired. Delete LevelFlow/Plate/TableFoodStand/HUD to rebuild.");
                return;
            }

            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainVR.unity");

            var oldEventSystem = GameObject.Find("EventSystem");
            if (oldEventSystem != null)
                Object.DestroyImmediate(oldEventSystem);

            var player = GameObject.Find("PlayerRig");
            if (player == null)
            {
                Debug.LogError("[NutriPick] MainVR has no PlayerRig.");
                return;
            }

            player.transform.position = new Vector3(0f, 0f, -2.0f);
            player.transform.rotation = Quaternion.identity;

            var controller = player.GetComponent<CharacterController>();
            if (controller == null)
                controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            var input = player.GetComponent<GamepadInput>();
            if (input == null)
                input = player.AddComponent<GamepadInput>();
            var fpc = player.GetComponent<FirstPersonController>();
            if (fpc == null)
                fpc = player.AddComponent<FirstPersonController>();
            var carrier = player.GetComponent<HeldFoodCarrier>();
            if (carrier == null)
                carrier = player.AddComponent<HeldFoodCarrier>();
            var reticle = player.GetComponent<ReticleInteractor>();
            if (reticle == null)
                reticle = player.AddComponent<ReticleInteractor>();

            var pivot = player.transform.Find("CameraPivot");
            if (pivot == null)
            {
                var pivotGo = new GameObject("CameraPivot");
                pivotGo.transform.SetParent(player.transform, false);
                pivot = pivotGo.transform;
            }
            pivot.localPosition = new Vector3(0f, 1.6f, 0f);

            var cameraTransform = pivot.Find("Main Camera");
            if (cameraTransform == null)
            {
                var cameraGo = new GameObject("Main Camera");
                cameraGo.tag = "MainCamera";
                cameraGo.transform.SetParent(pivot, false);
                cameraGo.AddComponent<Camera>();
                cameraGo.AddComponent<AudioListener>();
                cameraTransform = cameraGo.transform;
            }
            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = Quaternion.identity;
            cameraTransform.tag = "MainCamera";
            var camera = cameraTransform.GetComponent<Camera>();

            var hold = cameraTransform.Find("HoldAnchor");
            if (hold == null)
            {
                var holdGo = new GameObject("HoldAnchor");
                holdGo.transform.SetParent(cameraTransform, false);
                holdGo.transform.localPosition = new Vector3(0f, -0.15f, 0.5f);
                hold = holdGo.transform;
            }

            fpc.controller = controller;
            fpc.cameraPivot = pivot;
            fpc.input = input;
            carrier.holdAnchor = hold;
            reticle.interactionCamera = camera;
            reticle.carrier = carrier;

            Move("wooden_chair", new Vector3(-1.65f, 0f, -0.45f));
            Move("ceramic_plate", new Vector3(-0.68f, 1.34f, 0.55f));
            Move("ceramic_plate (1)", new Vector3(0.68f, 1.34f, 0.55f));
            Move("ceramic_plate (2)", new Vector3(-0.68f, 1.34f, 0.15f));
            Move("rendang", new Vector3(0f, 1.36f, 0.7f));
            Move("BURGER", new Vector3(0.68f, 1.36f, 0.15f));
            Move("mie", new Vector3(-0.28f, 1.46f, 0.7f));
            Move("telur", new Vector3(-0.44f, 1.46f, 0.7f));
            Move("telur ceplok", new Vector3(-0.58f, 1.34f, 0.7f));
            Move("picang", new Vector3(0.36f, 1.36f, 0.7f));
            Move("fah", new Vector3(-0.88f, 1.34f, 0.7f));

            var plateVisual = GameObject.Find("ceramic_plate (3)");
            if (plateVisual == null)
            {
                Debug.LogError("[NutriPick] MainVR has no ceramic_plate (3) to use as the serving Plate.");
                return;
            }

            var plateRoot = new GameObject("Plate");
            plateRoot.transform.position = new Vector3(0f, 1.34f, -0.55f);
            plateRoot.transform.rotation = Quaternion.identity;
            plateVisual.transform.SetParent(plateRoot.transform, false);
            plateVisual.transform.localPosition = Vector3.zero;

            var plate = plateRoot.AddComponent<PlateController>();
            plate.slots = CreateSlots(plateRoot.transform, plate);

            var tableSurface = new GameObject("TableSurfaceCollider");
            tableSurface.transform.position = new Vector3(-0.01f, 1.305f, 0.01f);
            var tableBox = tableSurface.AddComponent<BoxCollider>();
            tableBox.size = new Vector3(2.0f, 0.02f, 1.88f);

            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_Level01_Warteg.asset");
            var rules = AssetDatabase.LoadAssetAtPath<NutritionRules>("Assets/ScriptableObjects/Rules/NutritionRules.asset");
            if (level == null || rules == null)
            {
                Debug.LogError("[NutriPick] Missing LevelDefinition or NutritionRules asset.");
                return;
            }

            level.sceneName = "MainVR";
            level.nextSceneName = "Level02_Padang";
            level.displayName = "Level 1 — Warteg";
            EditorUtility.SetDirty(level);

            var standGo = new GameObject("TableFoodStand");
            var stand = standGo.AddComponent<FoodStand>();
            stand.displayName = "Meja Warteg";
            stand.foods = level.foodPool;
            stand.anchors = new Transform[level.foodPool.Length];
            standGo.SetActive(false);

            for (int i = 0; i < level.foodPool.Length; i++)
            {
                var food = level.foodPool[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Food/{food.id}.prefab");
                if (prefab == null)
                {
                    Debug.LogError($"[NutriPick] Missing food prefab for {food.id}.");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.position = new Vector3(-0.7f + i * 0.2f, 1.36f, 0.05f);
                instance.transform.rotation = Quaternion.identity;
                var item = instance.GetComponent<FoodItem>();
                item.SetHome(stand, instance.transform.position, instance.transform.rotation);
                stand.anchors[i] = instance.transform;
            }

            var servePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Interaction/ServeStation.prefab");
            var serve = (GameObject)PrefabUtility.InstantiatePrefab(servePrefab);
            serve.transform.position = new Vector3(0.78f, 1.36f, -0.55f);
            serve.transform.rotation = Quaternion.identity;

            var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/HUD.prefab");
            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);

            var flowGo = new GameObject("LevelFlow");
            var flow = flowGo.AddComponent<LevelFlowController>();
            flow.level = level;
            flow.firstSceneName = "MainVR";
            flow.rules = rules;
            flow.plate = plate;
            flow.stands = new[] { stand };
            flow.serveStation = serve.GetComponent<ServeStation>();
            flow.player = fpc;
            flow.input = input;
            flow.reticle = reticle;
            flow.bar = hud.GetComponentInChildren<NutritionBar>(true);
            flow.timer = hud.GetComponentInChildren<LevelTimerDisplay>(true);
            flow.prompt = hud.GetComponentInChildren<InteractionPrompt>(true);
            flow.infoCard = hud.GetComponentInChildren<FoodInfoCardPresenter>(true);
            flow.report = hud.GetComponentInChildren<HealthReportPresenter>(true);
            flow.recap = hud.GetComponentInChildren<RecapPresenter>(true);
            flow.pausePanel = FindDeepChild(hud.transform, "PausePanel")?.gameObject;
            flow.pauseMessage = FindDeepChild(hud.transform, "PauseMessage")?.GetComponent<TMP_Text>();
            flow.pauseResumeButton = FindDeepChild(hud.transform, "ResumeButton")?.GetComponent<Button>();
            flow.pauseQuitButton = FindDeepChild(hud.transform, "QuitButton")?.GetComponent<Button>();

            reticle.flow = flow;
            reticle.plate = plate;
            reticle.serveStation = flow.serveStation;
            reticle.infoCard = flow.infoCard;
            reticle.prompt = flow.prompt;

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainVR.unity");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainVR.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level02_Padang.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level03_Bazaar.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NutriPick] MainVR is now Level 1 with the existing Plate as the serving plate.");
        }

        private static PlateSlot[] CreateSlots(Transform plateRoot, PlateController plate)
        {
            var highlightMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SlotHighlight.mat");
            var positions = new[]
            {
                new Vector3(0.06f, 0.02f, 0.06f),
                new Vector3(-0.06f, 0.02f, 0.06f),
                new Vector3(0.06f, 0.02f, -0.06f),
                new Vector3(-0.06f, 0.02f, -0.06f)
            };

            var slots = new List<PlateSlot>();
            for (int i = 0; i < positions.Length; i++)
            {
                var slotGo = new GameObject("Slot" + (i + 1));
                slotGo.transform.SetParent(plateRoot, false);
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
                    Object.DestroyImmediate(highlightCollider);
                if (highlightMaterial != null)
                    highlight.GetComponent<Renderer>().sharedMaterial = highlightMaterial;
                highlight.SetActive(false);
                slot.highlight = highlight;

                slots.Add(slot);
            }

            return slots.ToArray();
        }

        private static void Move(string name, Vector3 position)
        {
            var go = GameObject.Find(name);
            if (go != null)
                go.transform.position = position;
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
