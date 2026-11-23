using UnityEditor;
using UnityEngine;
using NutriPick.Content;
using NutriPick.Nutrition;

namespace NutriPick.Editor
{
    public static class Scene1DataSetup
    {
        private static readonly (string id, ProteinType proteinType)[] Foods =
        {
            ("rendang", ProteinType.Hewani),
            ("burger", ProteinType.Hewani),
            ("telur_rebus", ProteinType.Hewani),
            ("telur_ceplok", ProteinType.Hewani),
            ("tempe", ProteinType.Nabati),
            ("brokoli", ProteinType.Nabati),
            ("pisang", ProteinType.Nabati),
            ("ayam_goreng", ProteinType.Hewani)
        };

        [MenuItem("NutriPick/Setup Scene 1 Data")]
        public static void Setup()
        {
            int updated = 0;
            foreach (var entry in Foods)
            {
                var path = $"Assets/ScriptableObjects/Food/FD_{entry.id}.asset";
                var food = AssetDatabase.LoadAssetAtPath<FoodData>(path);
                if (food == null)
                {
                    Debug.LogError($"[Scene1Data] Missing {path}");
                    continue;
                }

                food.hasProteinType = true;
                food.proteinType = entry.proteinType;
                EditorUtility.SetDirty(food);
                updated++;
            }

            var levelPath = "Assets/ScriptableObjects/Levels/LD_MainVR.asset";
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(levelPath);
            if (level != null)
            {
                level.sceneName = "MainVR";
                level.minItems = 4;
                level.requiresDrink = false;
                EditorUtility.SetDirty(level);
            }
            else
            {
                Debug.LogError($"[Scene1Data] Missing {levelPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Scene1Data] Updated {updated} foods and LD_MainVR.");
        }
    }
}
