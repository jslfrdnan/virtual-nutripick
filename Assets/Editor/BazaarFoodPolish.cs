using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NutriPick.Content;
using NutriPick.Interaction;

namespace NutriPick.Editor
{
    /// <summary>
    /// Visual polish for the Level 3 bazaar food:
    /// - gives ayam goreng a proper textured M_Food_* material (it used to fall back
    ///   to the untextured Material_0 embedded in the FBX, so it rendered white/grey);
    /// - spreads the duplicate portions of tempe, ayam goreng, brokoli and pisang so
    ///   they no longer interpenetrate into a single blob.
    /// </summary>
    public static class BazaarFoodPolish
    {
        private const string ScenePath = "Assets/Scenes/Level03_Bazaar.unity";
        private const string SceneRootName = "Scene3Bazaar";
        private const string AyamPrefabPath = "Assets/Prefabs/Food/ayam_goreng.prefab";
        private const string AlbedoPath = "Assets/Textures/Food/Home/M_Food_Ayam_Albedo.png";
        private const string MaterialPath = "Assets/Materials/Home/M_Food_Ayam.mat";

        private const float PortionGap = 0.012f;
        private const float MaxPortionRun = 0.34f;
        private const float MaxPlateDiameter = 0.30f;

        [MenuItem("NutriPick/Polish Bazaar Food")]
        public static void RunAll()
        {
            EnsureSceneOpen();
            BuildChickenMaterial();
            SpreadPortions();
            SaveScene();
        }

        [MenuItem("NutriPick/Polish Bazaar Food/Material Only")]
        public static void BuildChickenMaterial()
        {
            if (!File.Exists(AlbedoPath))
                GenerateCrustTexture(AlbedoPath);

            AssetDatabase.ImportAsset(AlbedoPath, ImportAssetOptions.ForceUpdate);
            ConfigureAlbedoImporter();

            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath);
            if (albedo == null)
            {
                Debug.LogError($"[BazaarFood] Missing albedo at {AlbedoPath}.");
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogError("[BazaarFood] URP/Unlit shader not found.");
                return;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            material.shader = shader;
            material.SetTexture("_BaseMap", albedo);
            material.SetTexture("_MainTex", albedo);
            material.SetTextureScale("_BaseMap", new Vector2(3f, 3f));
            material.SetTextureScale("_MainTex", new Vector2(3f, 3f));
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_Color", Color.white);
            material.SetFloat("_Smoothness", 0.12f);
            material.SetFloat("_Glossiness", 0.12f);
            EditorUtility.SetDirty(material);

            var root = PrefabUtility.LoadPrefabContents(AyamPrefabPath);
            int replaced = 0;

            try
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    var materials = renderer.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        var current = materials[i];
                        if (current == null || current.name == "Material_0")
                        {
                            materials[i] = material;
                            replaced++;
                        }
                    }

                    renderer.sharedMaterials = materials;
                }

                PrefabUtility.SaveAsPrefabAsset(root, AyamPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[BazaarFood] M_Food_Ayam applied to ayam_goreng.prefab ({replaced} material slot(s)).");
        }

        [MenuItem("NutriPick/Polish Bazaar Food/Spread Portions Only")]
        public static void SpreadPortions()
        {
            EnsureSceneOpen();

            var root = GameObject.Find(SceneRootName);
            if (root == null)
            {
                Debug.LogError($"[BazaarFood] Missing scene root '{SceneRootName}'.");
                return;
            }

            int groups = 0;
            int moved = 0;

            foreach (var stand in Object.FindObjectsByType<FoodStand>())
            {
                var foodRoot = root.transform.Find(stand.name + "_Food");
                if (foodRoot == null)
                {
                    Debug.LogWarning($"[BazaarFood] No food root for '{stand.name}'.");
                    continue;
                }

                var items = foodRoot.GetComponentsInChildren<FoodItem>(true)
                    .Where(item => item != null && item.data != null)
                    .GroupBy(item => item.data.id);

                foreach (var group in items)
                {
                    var list = group.ToList();
                    if (list.Count < 2)
                        continue;

                    groups++;
                    var anchor = stand.transform.Find("Anchor_" + group.Key);
                    Vector3 position = anchor != null ? anchor.position : Centroid(list);

                    var baseYaw = Quaternion.Euler(0f, stand.transform.eulerAngles.y, 0f);
                    var layout = ChooseLayout(list, position, baseYaw);

                    for (int i = 0; i < list.Count; i++)
                    {
                        int column = i % layout.columns;
                        int row = i / layout.columns;
                        float offsetX = layout.columns == 1
                            ? 0f
                            : (column - (layout.columns - 1) * 0.5f) * layout.stepX;
                        float offsetZ = (row - (layout.rows - 1) * 0.5f) * layout.stepZ;

                        var item = list[i];
                        var targetCenter = layout.center + new Vector3(offsetX, 0f, offsetZ);
                        item.transform.SetPositionAndRotation(
                            targetCenter - layout.offset,
                            layout.rotation);
                        moved++;
                    }

                    FitPlate(stand.transform, group.Key, layout.spanX, layout.spanZ, layout.center);
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[BazaarFood] Spread {moved} portion(s) across {groups} food group(s).");
        }

        private static void EnsureSceneOpen()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                EditorSceneManager.OpenScene(ScenePath);
        }

        private static void SaveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path == ScenePath && scene.isDirty)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                Debug.Log("[BazaarFood] Level03_Bazaar saved.");
            }
        }

        private static Vector3 Centroid(System.Collections.Generic.List<FoodItem> items)
        {
            var sum = Vector3.zero;
            foreach (var item in items)
                sum += item.transform.position;

            return sum / items.Count;
        }

        private struct ItemFootprint
        {
            public Vector3 size;
            public Vector3 offset;
        }

        private struct PortionLayout
        {
            public Quaternion rotation;
            public Vector3 offset;
            public Vector3 center;
            public float stepX;
            public float stepZ;
            public int columns;
            public int rows;
            public float spanX;
            public float spanZ;
        }

        private static PortionLayout ChooseLayout(
            System.Collections.Generic.List<FoodItem> items,
            Vector3 position,
            Quaternion baseYaw)
        {
            var sample = items[0];
            var layoutA = BuildLayout(items.Count, position, baseYaw, Describe(sample, baseYaw));

            var turnedYaw = baseYaw * Quaternion.Euler(0f, 90f, 0f);
            var layoutB = BuildLayout(items.Count, position, turnedYaw, Describe(sample, turnedYaw));

            float areaA = Mathf.Max(layoutA.spanX, layoutA.spanZ);
            float areaB = Mathf.Max(layoutB.spanX, layoutB.spanZ);
            return areaB < areaA - 0.001f ? layoutB : layoutA;
        }

        private static PortionLayout BuildLayout(int count, Vector3 position, Quaternion rotation, ItemFootprint footprint)
        {
            float stepZ = footprint.size.z + PortionGap;
            float stepX = footprint.size.x + PortionGap;
            bool singleRow = count * stepZ <= MaxPortionRun;
            int columns = singleRow ? 1 : 2;
            int rows = Mathf.CeilToInt(count / (float)columns);

            return new PortionLayout
            {
                rotation = rotation,
                offset = footprint.offset,
                center = position + footprint.offset,
                stepX = stepX,
                stepZ = stepZ,
                columns = columns,
                rows = rows,
                spanX = Mathf.Max(columns == 1 ? footprint.size.x : columns * stepX, footprint.size.x),
                spanZ = Mathf.Max(rows * stepZ, footprint.size.z)
            };
        }

        private static ItemFootprint Describe(FoodItem item, Quaternion rotation)
        {
            var previousRotation = item.transform.rotation;
            var previousPosition = item.transform.position;

            item.transform.rotation = rotation;
            Physics.SyncTransforms();

            var collider = item.itemCollider != null ? item.itemCollider : item.GetComponent<Collider>();
            var size = collider != null ? collider.bounds.size : new Vector3(0.1f, 0.1f, 0.1f);
            var offset = collider != null ? collider.bounds.center - item.transform.position : Vector3.zero;

            item.transform.SetPositionAndRotation(previousPosition, previousRotation);
            Physics.SyncTransforms();

            return new ItemFootprint { size = size, offset = offset };
        }

        private static void FitPlate(Transform stand, string foodId, float spanX, float spanZ, Vector3 center)
        {
            var plate = stand.Find("Dish_" + foodId);
            if (plate == null)
                return;

            var renderer = plate.GetComponentInChildren<Renderer>();
            if (renderer == null)
                return;

            plate.localScale = Vector3.one;
            float unit = renderer.bounds.size.z;

            if (unit <= 0.0001f)
                return;

            float wanted = Mathf.Clamp(Mathf.Max(spanX, spanZ) + 0.02f, 0.14f, MaxPlateDiameter);
            plate.localScale = Vector3.one * (wanted / unit);
            plate.position = new Vector3(center.x, plate.position.y, center.z);
        }

        private static void ConfigureAlbedoImporter()
        {
            var importer = AssetImporter.GetAtPath(AlbedoPath) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static void GenerateCrustTexture(string path)
        {
            const int size = 512;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            var dark = new Color(0.42f, 0.22f, 0.07f);
            var mid = new Color(0.74f, 0.46f, 0.16f);
            var light = new Color(0.94f, 0.72f, 0.36f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x / (float)size;
                    float v = y / (float)size;

                    float crust = 0.55f * ValueNoise(u, v, 11, 3)
                                  + 0.30f * ValueNoise(u, v, 37, 17)
                                  + 0.15f * ValueNoise(u, v, 97, 29);

                    Color color = crust < 0.5f
                        ? Color.Lerp(dark, mid, crust * 2f)
                        : Color.Lerp(mid, light, (crust - 0.5f) * 2f);

                    float speckle = Hash(x, y, size, 101);
                    if (speckle > 0.94f)
                        color = Color.Lerp(color, light, 0.7f);
                    else if (speckle < 0.05f)
                        color = Color.Lerp(color, dark, 0.55f);

                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "Assets");
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[BazaarFood] Generated crust albedo at {path}.");
        }

        private static float ValueNoise(float u, float v, int period, int seed)
        {
            float x = u * period;
            float y = v * period;
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float fx = x - x0;
            float fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);

            float a = Hash(x0, y0, period, seed);
            float b = Hash(x0 + 1, y0, period, seed);
            float c = Hash(x0, y0 + 1, period, seed);
            float d = Hash(x0 + 1, y0 + 1, period, seed);

            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        private static float Hash(int x, int y, int period, int seed)
        {
            x = ((x % period) + period) % period;
            y = ((y % period) + period) % period;

            uint hash = (uint)(x * 374761393 + y * 668265263 + seed * 69069);
            hash = (hash ^ (hash >> 13)) * 1274126177u;
            hash ^= hash >> 16;
            return (hash & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }
}
