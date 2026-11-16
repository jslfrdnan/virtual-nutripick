using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.Nutrition;

namespace NutriPick.Editor
{
    public static class MainVRFoodRowFixup
    {
        [MenuItem("NutriPick/Rebuild MainVR Food Row")]
        public static void RebuildFoodRow()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainVR.unity");
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/ScriptableObjects/Levels/LD_MainVR.asset");
            if (level == null)
            {
                Debug.LogError("[NutriPick] LD_Level01_Warteg.asset not found.");
                return;
            }

            var oldItems = new List<FoodItem>();
            foreach (var root in scene.GetRootGameObjects())
                oldItems.AddRange(root.GetComponentsInChildren<FoodItem>(true));
            foreach (var item in oldItems)
                Object.DestroyImmediate(item.gameObject);

            FoodStand stand = null;
            foreach (var candidate in Object.FindObjectsByType<FoodStand>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (candidate.gameObject.scene == scene && candidate.gameObject.name == "TableFoodStand")
                {
                    stand = candidate;
                    break;
                }
            }

            if (stand == null)
            {
                var standGo = new GameObject("TableFoodStand");
                stand = standGo.AddComponent<FoodStand>();
            }

            stand.displayName = "Meja Warteg";
            stand.foods = level.foodPool;
            stand.anchors = new Transform[level.foodPool.Length];
            stand.gameObject.SetActive(false);

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

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            if (flow != null)
            {
                flow.level = level;
                flow.stands = new[] { stand };
            }

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainVR.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[NutriPick] MainVR food row rebuilt from Assets/Models/Food prefabs.");
        }
    }
}
