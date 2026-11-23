using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NutriPick.Content;
using NutriPick.Flow;
using NutriPick.Interaction;
using NutriPick.Nutrition;

namespace NutriPick.Editor
{
    /// <summary>
    /// Converts Level 3 from eight single-food stalls into two open stalls
    /// (Healthy Food and Junk Food) while the remaining stalls become closed decor.
    /// </summary>
    public static class Scene3BazaarTwoStandPatcher
    {
        private const string ScenePath = "Assets/Scenes/Level03_Bazaar.unity";
        private const string SceneRootName = "Scene3Bazaar";
        private const string HealthyStandName = "BazaarStand_HealthyFood";
        private const string JunkStandName = "BazaarStand_JunkFood";
        private const string HealthyDisplayName = "Healthy Food";
        private const string JunkDisplayName = "Junk Food";

        private static readonly string[] HealthyIds =
        {
            "brokoli", "pisang", "tempe", "telur_rebus"
        };

        private static readonly string[] JunkIds =
        {
            "mie_goreng", "burger"
        };

        private static readonly Vector3 HealthyPosition = new Vector3(-14.5f, 0f, 28f);
        private static readonly Vector3 JunkPosition = new Vector3(-3.5f, 0f, -23f);
        private const float HealthyYaw = 90f;
        private const float JunkYaw = -90f;

        [MenuItem("NutriPick/Apply Two Open Stands (Level 3)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath);

            var root = GameObject.Find(SceneRootName);
            if (root == null)
            {
                Debug.LogError($"[TwoStands] Missing scene root '{SceneRootName}'.");
                return;
            }

            if (root.transform.Find(HealthyStandName) != null && root.transform.Find(JunkStandName) != null)
            {
                Debug.Log("[TwoStands] Level 3 already uses two open stands.");
                return;
            }

            var decorTemplate = root.transform.Find("DecorStall_Flamboyant_01");
            if (decorTemplate == null)
            {
                Debug.LogError("[TwoStands] Missing closed decor template 'DecorStall_Flamboyant_01'.");
                return;
            }

            var allIds = HealthyIds.Concat(JunkIds).ToArray();
            var foods = Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(item => item != null && item.data != null)
                .GroupBy(item => item.data.id)
                .ToDictionary(group => group.Key, group => group.First());

            var sourceStands = new Dictionary<string, GameObject>();
            var dishes = new Dictionary<string, GameObject>();
            var oldFoodRoots = new Dictionary<string, GameObject>();

            foreach (var id in allIds)
            {
                var stand = root.transform.Find("BazaarStand_" + id);
                if (stand == null)
                {
                    Debug.LogError($"[TwoStands] Missing source stand 'BazaarStand_{id}'.");
                    return;
                }

                sourceStands[id] = stand.gameObject;

                var dish = stand.Find("Dish");
                if (dish != null)
                    dishes[id] = dish.gameObject;

                var foodRoot = root.transform.Find("BazaarStand_" + id + "_Food");
                if (foodRoot != null)
                    oldFoodRoots[id] = foodRoot.gameObject;
            }

            var healthyStand = sourceStands["mie_goreng"];
            var junkStand = sourceStands["burger"];

            // Park every dish outside the stands before rebuilding, otherwise a dish that
            // belongs to the other group is destroyed with its old stand's children.
            var dishHolder = new GameObject("__DishHolder");
            dishHolder.transform.SetParent(root.transform, false);
            foreach (var dish in dishes.Values)
            {
                if (dish != null)
                    dish.transform.SetParent(dishHolder.transform, true);
            }

            var standsToClose = new List<GameObject>();
            foreach (Transform child in root.transform)
            {
                if (!child.name.StartsWith("BazaarStand_") || child.name.EndsWith("_Food"))
                    continue;
                if (child.gameObject == healthyStand || child.gameObject == junkStand)
                    continue;
                standsToClose.Add(child.gameObject);
            }

            var healthyStandComponent = ConfigureStand(
                healthyStand, HealthyStandName, HealthyDisplayName,
                HealthyPosition, HealthyYaw, FoodStandKind.Healthy, HealthyIds,
                foods, dishes, root.transform, HealthyStandName + "_Food");

            var junkStandComponent = ConfigureStand(
                junkStand, JunkStandName, JunkDisplayName,
                JunkPosition, JunkYaw, FoodStandKind.Junk, JunkIds,
                foods, dishes, root.transform, JunkStandName + "_Food");

            foreach (var foodRoot in oldFoodRoots.Values)
            {
                if (foodRoot != null)
                    Object.DestroyImmediate(foodRoot);
            }

            Object.DestroyImmediate(dishHolder);

            int closedIndex = 1;
            foreach (var oldStand in standsToClose)
            {
                if (oldStand == null)
                    continue;

                Vector3 position = oldStand.transform.position;
                Quaternion rotation = oldStand.transform.rotation;
                Object.DestroyImmediate(oldStand);

                var closed = Object.Instantiate(decorTemplate.gameObject, root.transform);
                closed.name = "DecorStall_Closed_" + closedIndex.ToString("00");
                closed.transform.position = position;
                closed.transform.rotation = rotation;

                var decor = closed.GetComponent<DecorStall>();
                if (decor != null)
                    decor.displayName = closed.name;

                closedIndex++;
            }

            var flow = Object.FindFirstObjectByType<LevelFlowController>();
            if (flow != null)
            {
                flow.stands = new[] { healthyStandComponent, junkStandComponent };
                EditorUtility.SetDirty(flow);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            int openFoods = (healthyStandComponent.foods?.Length ?? 0) + (junkStandComponent.foods?.Length ?? 0);
            int decorCount = Object.FindObjectsByType<DecorStall>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            Debug.Log($"[TwoStands] Applied: 2 open stands, {openFoods} foods, {decorCount} closed stalls.");
        }

        private static FoodStand ConfigureStand(
            GameObject stand,
            string standName,
            string displayName,
            Vector3 position,
            float yaw,
            FoodStandKind kind,
            string[] ids,
            Dictionary<string, FoodItem> foods,
            Dictionary<string, GameObject> dishes,
            Transform sceneRoot,
            string foodRootName)
        {
            stand.name = standName;
            stand.transform.position = position;
            stand.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            var foodStand = stand.GetComponent<FoodStand>();
            if (foodStand == null)
                foodStand = stand.AddComponent<FoodStand>();

            var children = stand.transform.Cast<Transform>().ToList();
            foreach (var child in children)
            {
                string childName = child.name;
                bool keep = childName == "Model"
                    || childName.StartsWith("Collider_")
                    || childName.StartsWith("Sign");
                if (!keep)
                    Object.DestroyImmediate(child.gameObject);
            }

            foreach (var text in stand.GetComponentsInChildren<TextMeshPro>(true))
                text.text = displayName;

            string signKey = displayName.Replace(" ", string.Empty);
            foreach (Transform child in stand.transform)
            {
                if (child.name.StartsWith("SignFront_"))
                    child.name = "SignFront_" + signKey;
                else if (child.name.StartsWith("Sign_"))
                    child.name = "Sign_" + signKey;
            }

            var foodRoot = new GameObject(foodRootName);
            foodRoot.transform.SetParent(sceneRoot, false);

            var anchors = new Transform[ids.Length];
            var foodData = new FoodData[ids.Length];

            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i];
                float x = ItemX(ids.Length, i);
                float z = ItemZ(ids.Length, i);

                var anchor = new GameObject("Anchor_" + id);
                anchor.transform.SetParent(stand.transform, false);
                anchor.transform.localPosition = new Vector3(x, 0.98f, z);
                anchor.transform.localRotation = Quaternion.identity;
                anchors[i] = anchor.transform;

                if (dishes.TryGetValue(id, out var dish) && dish != null)
                {
                    dish.name = "Dish_" + id;
                    dish.transform.SetParent(stand.transform, false);
                    dish.transform.localPosition = new Vector3(x, 0.95f, z);
                    dish.transform.localRotation = Quaternion.identity;
                    dish.transform.localScale = Vector3.one * 0.7f;
                }

                if (!foods.TryGetValue(id, out var item) || item == null)
                {
                    Debug.LogError($"[TwoStands] Missing FoodItem for '{id}'.");
                    continue;
                }

                foodData[i] = item.data;
                item.homeStand = foodStand;
                item.transform.SetParent(foodRoot.transform, true);
                item.transform.position = anchor.transform.position;
                item.transform.rotation = anchor.transform.rotation;
            }

            foodStand.displayName = displayName;
            foodStand.kind = kind;
            foodStand.foods = foodData;
            foodStand.anchors = anchors;
            foodStand.enabled = false;
            foodStand.signText = stand.GetComponentsInChildren<TextMeshPro>(true).FirstOrDefault();

            EditorUtility.SetDirty(foodStand);
            return foodStand;
        }

        private static float ItemX(int count, int index)
        {
            if (count >= 5)
                return index < 3 ? (index - 1) * 0.42f : (index - 3 - 0.5f) * 0.42f;

            return (index - (count - 1) * 0.5f) * 0.45f;
        }

        private static float ItemZ(int count, int index)
        {
            // Keep food clear of the decorative produce crates at both ends of the counter.
            if (count < 5)
                return 0.85f;

            return index >= 3 ? 1.07f : 0.63f;
        }
    }
}
