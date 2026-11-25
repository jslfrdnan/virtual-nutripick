using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NutriPick.Interaction;
using NutriPick.Nutrition;

namespace NutriPick.Editor
{
    public static class QuestFoodBuilder
    {
        private const string ModelFolder = "Assets/Models/Food/Home";
        private const string PrefabFolder = "Assets/Prefabs/Food";
        private const string FoodFolder = "Assets/ScriptableObjects/Food";
        private const string IconFolder = "Assets/Textures/FoodIcons";

        private struct FoodSpec
        {
            public string id;
            public string displayName;
            public string description;
            public FoodCompositionCategory composition;
            public int calories;
            public float protein;
            public float carbs;
            public float fat;
            public float fiber;
            public float addedSugar;
            public float sodium;
            public float vitaminA;
            public float vitaminC;
            public float vitaminD;
            public float calcium;
            public float iron;
            public string portion;
        }

        private static readonly FoodSpec[] Foods =        {
            new FoodSpec
            {
                id = "tahu", displayName = "Tahu", portion = "100 g",
                description = "Protein nabati yang lembut dan kaya kalsium. Cocok untuk sarapan atau lauk makan malam.",
                composition = FoodCompositionCategory.Protein,
                calories = 80, protein = 8f, carbs = 2f, fat = 4f, fiber = 1f,
                addedSugar = 0f, sodium = 10f,
                vitaminA = 0f, vitaminC = 0f, vitaminD = 0f, calcium = 350f, iron = 1.5f
            },
            new FoodSpec
            {
                id = "jeruk", displayName = "Jeruk", portion = "1 buah",
                description = "Buah segar sumber vitamin C dan serat. Baik untuk daya tahan tubuh.",
                composition = FoodCompositionCategory.Fruit,
                calories = 62, protein = 1.2f, carbs = 15f, fat = 0.2f, fiber = 3.1f,
                addedSugar = 12f, sodium = 0f,
                vitaminA = 15f, vitaminC = 70f, vitaminD = 0f, calcium = 60f, iron = 0.1f
            },
            new FoodSpec
            {
                id = "wortel", displayName = "Wortel", portion = "100 g",
                description = "Sayur oranye kaya vitamin A untuk mata dan kulit.",
                composition = FoodCompositionCategory.Vegetable,
                calories = 41, protein = 0.9f, carbs = 10f, fat = 0.2f, fiber = 2.8f,
                addedSugar = 0f, sodium = 70f,
                vitaminA = 830f, vitaminC = 6f, vitaminD = 0f, calcium = 33f, iron = 0.3f
            },
            new FoodSpec
            {
                id = "bayam", displayName = "Bayam", portion = "100 g",
                description = "Sayur hijau sumber zat besi, kalsium, dan vitamin A.",
                composition = FoodCompositionCategory.Vegetable,
                calories = 23, protein = 2.9f, carbs = 3.6f, fat = 0.4f, fiber = 2.2f,
                addedSugar = 0f, sodium = 80f,
                vitaminA = 290f, vitaminC = 28f, vitaminD = 0f, calcium = 99f, iron = 2.7f
            },
            new FoodSpec
            {
                id = "ubi_jalar", displayName = "Ubi jalar", portion = "150 g",
                description = "Karbohidrat kompleks dengan vitamin A dan serat.",
                composition = FoodCompositionCategory.Carbohydrate,
                calories = 150, protein = 2f, carbs = 34f, fat = 0.2f, fiber = 4f,
                addedSugar = 6f, sodium = 40f,
                vitaminA = 700f, vitaminC = 20f, vitaminD = 0f, calcium = 45f, iron = 0.9f
            },
            new FoodSpec
            {
                id = "ikan_cakalang", displayName = "Ikan cakalang", portion = "100 g",
                description = "Ikan laut kaya protein, vitamin D, dan lemak baik.",
                composition = FoodCompositionCategory.Protein,
                calories = 130, protein = 22f, carbs = 0f, fat = 4.5f, fiber = 0f,
                addedSugar = 0f, sodium = 90f,
                vitaminA = 10f, vitaminC = 0f, vitaminD = 5.5f, calcium = 30f, iron = 1.2f
            }
        };

        private static readonly Dictionary<string, Color> MaterialColors = new Dictionary<string, Color>
        {
            { "M_Food_Tahu", new Color(0.93f, 0.90f, 0.76f) },
            { "M_Food_TahuLeaf", new Color(0.10f, 0.35f, 0.08f) },
            { "M_Food_Wortel", new Color(0.95f, 0.36f, 0.06f) },
            { "M_Food_Daun", new Color(0.16f, 0.44f, 0.10f) },
            { "M_Food_Jeruk", new Color(0.96f, 0.48f, 0.06f) },
            { "M_Food_Bayam", new Color(0.10f, 0.38f, 0.10f) },
            { "M_Food_Batang", new Color(0.38f, 0.55f, 0.14f) },
            { "M_Food_UbiKulit", new Color(0.42f, 0.24f, 0.28f) },
            { "M_Food_UbiIsi", new Color(0.55f, 0.24f, 0.45f) },
            { "M_Food_IkanBadan", new Color(0.38f, 0.45f, 0.55f) },
            { "M_Food_IkanPerut", new Color(0.78f, 0.80f, 0.82f) },
            { "M_Food_IkanSirip", new Color(0.20f, 0.24f, 0.30f) },
            { "M_Food_Mata", new Color(0.95f, 0.95f, 0.92f) },
            { "M_Food_Pupil", new Color(0.03f, 0.03f, 0.03f) }
        };

        [MenuItem("NutriPick/Build Quest Foods")]
        public static void Build()
        {
            foreach (var spec in Foods)
            {
                ConfigureModel(spec.id);
                ConfigureIcon(spec.id);
            }

            AssetDatabase.Refresh();
            foreach (var spec in Foods)
                BuildFood(spec);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[QuestFoods] Built 6 food prefabs, data assets, and icons.");
        }

        private static void ConfigureModel(string id)
        {
            string path = $"{ModelFolder}/Food_{id}.fbx";
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogError($"[QuestFoods] Missing model {path}");
                return;
            }

            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
        }

        private static void ConfigureIcon(string id)
        {
            string path = $"{IconFolder}/{id}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[QuestFoods] Missing icon {path}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }

        private static void BuildFood(FoodSpec spec)
        {
            string modelPath = $"{ModelFolder}/Food_{spec.id}.fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                Debug.LogError($"[QuestFoods] Model not imported: {modelPath}");
                return;
            }

            var data = LoadOrCreateFood(spec);
            var root = new GameObject(spec.id);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            ApplyUnlitMaterials(root);

            var bounds = CalculateBounds(root);
            var body = root.AddComponent<Rigidbody>();
            body.mass = 0.2f;
            body.linearDamping = 0.4f;
            body.angularDamping = 0.4f;
            body.useGravity = true;
            body.isKinematic = true;

            var collider = root.AddComponent<BoxCollider>();
            collider.center = bounds.center;
            collider.size = bounds.size == Vector3.zero ? Vector3.one * 0.1f : bounds.size;

            var item = root.AddComponent<FoodItem>();
            item.data = data;
            item.body = body;
            item.itemCollider = collider;

            string prefabPath = $"{PrefabFolder}/{spec.id}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            data.prefab = prefab;
            data.icon = AssetDatabase.LoadAssetAtPath<Sprite>($"{IconFolder}/{spec.id}.png");
            EditorUtility.SetDirty(data);
            Debug.Log($"[QuestFoods] {spec.id}: bounds={bounds.size}");
        }

        private static void ApplyUnlitMaterials(GameObject root)
        {
            var cache = new Dictionary<string, Material>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null)
                        continue;

                    string key = materials[i].name.Replace(" (Instance)", string.Empty);
                    if (!cache.TryGetValue(key, out var replacement))
                    {
                        replacement = CreateUnlitMaterial(key);
                        cache[key] = replacement;
                    }

                    materials[i] = replacement;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static Material CreateUnlitMaterial(string name)
        {
            string path = $"Assets/Materials/Food/Quest/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;

            EnsureFolder("Assets/Materials/Food/Quest");
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogError("[QuestFoods] Missing URP/Unlit shader.");
                return null;
            }

            var material = new Material(shader);
            material.SetTexture("_BaseMap", GetOrCreateTexture(name));
            material.SetColor("_BaseColor", Color.white);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Texture2D GetOrCreateTexture(string name)
        {
            string path = $"Assets/Textures/Food/Quest/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null)
                return existing;

            EnsureFolder("Assets/Textures/Food/Quest");
            var color = MaterialColors.TryGetValue(name, out var value)
                ? value
                : new Color(0.7f, 0.7f, 0.7f);

            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            var pixels = new Color32[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            texture.SetPixels32(pixels);
            texture.Apply();

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            if (!string.IsNullOrEmpty(parent))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.one * 0.1f);

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            var localCenter = root.transform.InverseTransformPoint(bounds.center);
            return new Bounds(localCenter, bounds.size);
        }

        private static FoodData LoadOrCreateFood(FoodSpec spec)
        {
            string path = $"{FoodFolder}/FD_{spec.id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<FoodData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<FoodData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.id = spec.id;
            data.displayName = spec.displayName;
            data.description = spec.description;
            data.composition = spec.composition;
            data.processing = ProcessingLevel.Minimal;
            data.isFried = false;
            data.isDrink = false;
            data.healthyOverride = HealthyOverride.None;
            data.calories = spec.calories;
            data.protein = spec.protein;
            data.carbs = spec.carbs;
            data.fat = spec.fat;
            data.fiber = spec.fiber;
            data.addedSugar = spec.addedSugar;
            data.sodium = spec.sodium;
            data.portionLabel = spec.portion;
            data.hasProteinType = spec.composition == FoodCompositionCategory.Protein;
            data.proteinType = spec.id == "tahu" ? ProteinType.Nabati : ProteinType.Hewani;
            data.vitaminA = spec.vitaminA;
            data.vitaminC = spec.vitaminC;
            data.vitaminD = spec.vitaminD;
            data.calcium = spec.calcium;
            data.iron = spec.iron;
            EditorUtility.SetDirty(data);
            return data;
        }
    }
}
