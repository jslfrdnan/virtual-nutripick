using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NutriPick.Nutrition;
using NutriPick.Quests;

namespace NutriPick.Editor
{
    public static class QuestDataBuilder
    {
        private const string QuestFolder = "Assets/ScriptableObjects/Quests";
        private const string FoodFolder = "Assets/ScriptableObjects/Food";

        private struct Micro
        {
            public float vitaminA;
            public float vitaminC;
            public float vitaminD;
            public float calcium;
            public float iron;

            public Micro(float a, float c, float d, float calcium, float iron)
            {
                vitaminA = a;
                vitaminC = c;
                vitaminD = d;
                this.calcium = calcium;
                this.iron = iron;
            }
        }

        private static readonly Dictionary<string, Micro> MicroValues = new Dictionary<string, Micro>
        {
            { "nasi_putih", new Micro(0f, 0f, 0f, 10f, 0.4f) },
            { "telur_rebus", new Micro(80f, 0f, 1.1f, 25f, 0.9f) },
            { "telur_ceplok", new Micro(95f, 0f, 1.2f, 28f, 0.9f) },
            { "ayam_goreng", new Micro(30f, 0f, 0.2f, 15f, 1.0f) },
            { "rendang", new Micro(10f, 0f, 0f, 20f, 1.8f) },
            { "tempe", new Micro(0f, 0f, 0f, 111f, 2.7f) },
            { "tahu", new Micro(0f, 0f, 0f, 350f, 1.5f) },
            { "brokoli", new Micro(31f, 45f, 0f, 47f, 0.7f) },
            { "bayam", new Micro(290f, 28f, 0f, 99f, 2.7f) },
            { "wortel", new Micro(830f, 6f, 0f, 33f, 0.3f) },
            { "sayur_singkong", new Micro(20f, 15f, 0f, 25f, 0.6f) },
            { "pisang", new Micro(4f, 10f, 0f, 6f, 0.3f) },
            { "jeruk", new Micro(15f, 70f, 0f, 60f, 0.1f) },
            { "kerupuk", new Micro(0f, 0f, 0f, 5f, 0.2f) },
            { "burger", new Micro(40f, 2f, 0.5f, 90f, 2.5f) },
            { "mie_goreng", new Micro(20f, 1f, 0f, 40f, 1.5f) },
            { "ikan_cakalang", new Micro(10f, 0f, 5.5f, 30f, 1.2f) },
            { "ubi_jalar", new Micro(700f, 20f, 0f, 45f, 0.9f) }
        };

        [MenuItem("NutriPick/Build Quest Data")]
        public static void Build()
        {
            EnsureFolder(QuestFolder);
            BuildQuests();
            PatchFoodMicros();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[QuestData] Built 3 quests and patched food micronutrients.");
        }

        public static QuestDefinition[] LoadQuests()
        {
            var guids = AssetDatabase.FindAssets("t:QuestDefinition", new[] { QuestFolder });
            var quests = new List<QuestDefinition>();
            foreach (var guid in guids)
            {
                var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (quest != null)
                    quests.Add(quest);
            }

            quests.Sort((a, b) => a.group.CompareTo(b.group));
            return quests.ToArray();
        }

        private static void BuildQuests()
        {
            var anak = CreateQuest(
                "anak",
                "Anak-anak",
                "7-9 tahun",
                "Masa pertumbuhan: kalsium, vitamin D, protein, dan vitamin A.",
                QuestGroup.Anak,
                new AkgProfile
                {
                    calories = 1850f, protein = 45f, carbohydrate = 280f, fat = 60f,
                    fiber = 26f, addedSugar = 25f, sodium = 1200f,
                    vitaminA = 600f, vitaminC = 45f, vitaminD = 15f, calcium = 1000f, iron = 10f
                },
                new[]
                {
                    Level("Sarapan", FoodCompositionCategory.Protein, FoodCompositionCategory.Fruit,
                        NutrientType.Protein, 13f, NutrientType.Calcium, 280f),
                    Level("Makan siang", FoodCompositionCategory.Protein, FoodCompositionCategory.Vegetable,
                        NutrientType.VitaminA, 170f, NutrientType.Fiber, 7f),
                    Level("Makan malam", FoodCompositionCategory.Protein, FoodCompositionCategory.Vegetable,
                        NutrientType.Calcium, 280f, NutrientType.VitaminC, 13f)
                });
            anak.plateMinItems = 4;
            anak.plateScale = 0.8f;
            EditorUtility.SetDirty(anak);

            var dewasa = CreateQuest(
                "dewasa",
                "Dewasa",
                "19-29 tahun",
                "Produktivitas harian: protein, serat, vitamin C, dan zat besi.",
                QuestGroup.Dewasa,
                new AkgProfile
                {
                    calories = 2250f, protein = 60f, carbohydrate = 360f, fat = 65f,
                    fiber = 30f, addedSugar = 50f, sodium = 1500f,
                    vitaminA = 600f, vitaminC = 75f, vitaminD = 15f, calcium = 1000f, iron = 12f
                },
                new[]
                {
                    Level("Sarapan", FoodCompositionCategory.Protein, FoodCompositionCategory.Fruit,
                        NutrientType.Protein, 20f, NutrientType.Fiber, 10f),
                    Level("Makan siang", FoodCompositionCategory.Protein, FoodCompositionCategory.Vegetable,
                        NutrientType.VitaminC, 25f, NutrientType.Iron, 4f),
                    Level("Makan malam", FoodCompositionCategory.Protein, FoodCompositionCategory.Vegetable,
                        NutrientType.Protein, 20f, NutrientType.Fiber, 10f)
                });

            var lansia = CreateQuest(
                "lansia",
                "Lansia",
                "60-69 tahun",
                "Menjaga kekuatan tulang dan otot: protein, kalsium, vitamin D, serat.",
                QuestGroup.Lansia,
                new AkgProfile
                {
                    calories = 1700f, protein = 60f, carbohydrate = 275f, fat = 50f,
                    fiber = 25f, addedSugar = 25f, sodium = 1200f,
                    vitaminA = 600f, vitaminC = 75f, vitaminD = 20f, calcium = 1000f, iron = 9f
                },
                new[]
                {
                    Level("Sarapan", FoodCompositionCategory.Protein, FoodCompositionCategory.Fruit,
                        NutrientType.Protein, 20f, NutrientType.Calcium, 350f),
                    Level("Makan siang", FoodCompositionCategory.Protein, FoodCompositionCategory.Vegetable,
                        NutrientType.VitaminD, 7f, NutrientType.Fiber, 8f),
                    Level("Makan malam", FoodCompositionCategory.Protein, FoodCompositionCategory.Vegetable,
                        NutrientType.Protein, 20f, NutrientType.Calcium, 350f)
                });

            Debug.Log($"[QuestData] Quests ready: {anak.displayName}, {dewasa.displayName}, {lansia.displayName}");
        }

        private static QuestDefinition CreateQuest(
            string id,
            string displayName,
            string ageRange,
            string description,
            QuestGroup group,
            AkgProfile akg,
            QuestLevelTasks[] levels)
        {
            string path = $"{QuestFolder}/QD_{id}.asset";
            var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
            if (quest == null)
            {
                quest = ScriptableObject.CreateInstance<QuestDefinition>();
                AssetDatabase.CreateAsset(quest, path);
            }

            quest.id = id;
            quest.displayName = displayName;
            quest.ageRange = ageRange;
            quest.description = description;
            quest.group = group;
            quest.plateMinItems = 0;
            quest.plateScale = 1f;
            quest.dailyAkg = akg;
            quest.levels = levels;
            EditorUtility.SetDirty(quest);
            return quest;
        }

        private static QuestLevelTasks Level(
            string label,
            FoodCompositionCategory requiredA,
            FoodCompositionCategory requiredB,
            NutrientType nutrientA,
            float minimumA,
            NutrientType nutrientB,
            float minimumB)
        {
            return new QuestLevelTasks
            {
                label = label,
                requiredA = requiredA,
                requiredB = requiredB,
                targetA = new QuestNutrientTarget { nutrient = nutrientA, minimum = minimumA },
                targetB = new QuestNutrientTarget { nutrient = nutrientB, minimum = minimumB }
            };
        }

        private static void PatchFoodMicros()
        {
            var guids = AssetDatabase.FindAssets("t:FoodData", new[] { FoodFolder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var food = AssetDatabase.LoadAssetAtPath<FoodData>(path);
                if (food == null || !MicroValues.TryGetValue(food.id, out var micro))
                    continue;

                food.vitaminA = micro.vitaminA;
                food.vitaminC = micro.vitaminC;
                food.vitaminD = micro.vitaminD;
                food.calcium = micro.calcium;
                food.iron = micro.iron;
                EditorUtility.SetDirty(food);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            if (!string.IsNullOrEmpty(parent))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
