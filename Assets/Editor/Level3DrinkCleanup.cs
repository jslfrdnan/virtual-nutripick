using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NutriPick.Content;
using NutriPick.Interaction;
using NutriPick.Nutrition;

namespace NutriPick.Editor
{
    /// <summary>
    /// One-shot, idempotent cleanup for the decision that every level is drink-free.
    /// Removes the two drink FoodItems, their dishes/anchors, and the drink entries
    /// from every open stand and from the Level 3 pool without rebuilding the scene.
    /// </summary>
    public static class Level3DrinkCleanup
    {
        private const string ScenePath = "Assets/Scenes/Level03_Bazaar.unity";
        private const string LevelPath = "Assets/ScriptableObjects/Levels/LD_Level03_Bazaar.asset";
        private const string Level2Path = "Assets/ScriptableObjects/Levels/LD_Level02_Warteg.asset";
        private const string Level1Path = "Assets/ScriptableObjects/Levels/LD_MainVR.asset";

        private static readonly HashSet<string> DrinkIds = new HashSet<string>
        {
            "air_putih",
            "teh_manis"
        };

        [MenuItem("NutriPick/Patch Level 3 Drinks")]
        public static void Apply()
        {
            var active = EditorSceneManager.GetActiveScene();
            var scene = active.path == ScenePath
                ? active
                : EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int removedItems = 0;
            foreach (var item in Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Include))
            {
                if (item == null)
                    continue;

                string id = item.data != null ? item.data.id : item.name;
                if (!DrinkIds.Contains(id) && !DrinkIds.Contains(item.name))
                    continue;

                Object.DestroyImmediate(item.gameObject);
                removedItems++;
            }

            int removedDishes = 0;
            foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (transform == null)
                    continue;

                string name = transform.name;
                if (name == "Dish_air_putih" || name == "Dish_teh_manis" ||
                    name == "Anchor_air_putih" || name == "Anchor_teh_manis")
                {
                    Object.DestroyImmediate(transform.gameObject);
                    removedDishes++;
                }
            }

            foreach (var stand in Object.FindObjectsByType<FoodStand>(FindObjectsInactive.Include))
            {
                if (stand == null)
                    continue;

                var foods = stand.foods ?? new FoodData[0];
                var anchors = stand.anchors ?? new Transform[0];
                if (!foods.Any(food => food != null && DrinkIds.Contains(food.id)))
                    continue;

                var keptFoods = new List<FoodData>();
                var keptAnchors = new List<Transform>();
                for (int i = 0; i < foods.Length; i++)
                {
                    if (foods[i] != null && DrinkIds.Contains(foods[i].id))
                        continue;

                    keptFoods.Add(foods[i]);
                    keptAnchors.Add(i < anchors.Length ? anchors[i] : null);
                }

                stand.foods = keptFoods.ToArray();
                stand.anchors = keptAnchors.ToArray();
                EditorUtility.SetDirty(stand);
            }

            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath);
            if (level != null)
            {
                level.foodPool = (level.foodPool ?? new FoodData[0])
                    .Where(food => food != null && !DrinkIds.Contains(food.id))
                    .ToArray();
                level.requiresDrink = false;
                level.learningFocus = new[] { "variasi", "pilihan seimbang" };
                EditorUtility.SetDirty(level);
            }

            foreach (var path in new[] { Level1Path, Level2Path })
            {
                var other = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (other == null)
                    continue;

                other.requiresDrink = false;
                EditorUtility.SetDirty(other);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            int remainingFoods = Object.FindObjectsByType<FoodItem>(FindObjectsInactive.Include).Length;
            Debug.Log($"[DrinkCleanup] removed {removedItems} food items and {removedDishes} dish/anchor objects; {remainingFoods} foods remain in Level 3.");
        }
    }
}
