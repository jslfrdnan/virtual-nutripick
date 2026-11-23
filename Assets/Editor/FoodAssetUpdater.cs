using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NutriPick.Editor
{
    /// <summary>
    /// Applies the rebuilt food models and gives the five still-untextured foods a
    /// proper URP/Unlit material with an albedo, matching the rest of the food set.
    ///
    /// Models come from Blender/Food/scripts/update_food_models.py:
    ///   Food_ayam_goreng (decimated), Food_kerupuk, Food_nasi_putih, Food_sayur_singkong.
    /// </summary>
    public static class FoodAssetUpdater
    {
        private const string MaterialFolder = "Assets/Materials/Home";
        private const string TextureFolder = "Assets/Textures/Food/Home";
        private const string ModelFolder = "Assets/Models/Food/Home";
        private const string PrefabFolder = "Assets/Prefabs/Food";

        [MenuItem("NutriPick/Food/Apply Rebuilt Models And Materials")]
        public static void ApplyAll()
        {
            BuildProceduralTextures();
            BuildMaterials();
            UpdateModelPrefabs();
            FixBroccoliMaterials();
            AssetDatabase.SaveAssets();
            Debug.Log("[FoodAssets] Models, materials and colliders updated.");
        }

        private static void FixBroccoliMaterials()
        {
            // Half the florets used the pale lettuce crop, which made the broccoli
            // look white next to the green ones.
            const string prefabPath = "Assets/Prefabs/Food/brokoli.prefab";
            var broccoli = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/M_Food_Broccoli.mat");
            if (broccoli == null)
                return;

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var renderer = root.GetComponentInChildren<Renderer>(true);
                if (renderer == null)
                    return;

                var materials = renderer.sharedMaterials;
                int replaced = 0;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null && materials[i].name == "M_Food_Lettuce")
                    {
                        materials[i] = broccoli;
                        replaced++;
                    }
                }

                if (replaced == 0)
                    return;

                renderer.sharedMaterials = materials;
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[FoodAssets] brokoli: {replaced} pale floret material slot(s) switched to M_Food_Broccoli.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("NutriPick/Food/Build Textures Only")]
        public static void BuildProceduralTextures()
        {
            WriteTextureIfMissing("M_Food_Kerupuk_Albedo", GenerateCrackerTexture);
            WriteTextureIfMissing("M_Food_NasiPutih_Albedo", GenerateRiceTexture);
            AssetDatabase.Refresh();
        }

        [MenuItem("NutriPick/Food/Build Materials Only")]
        public static void BuildMaterials()
        {
            CreateMaterial("M_Food_Kerupuk", "M_Food_Kerupuk_Albedo", new Vector2(2f, 2f));
            CreateMaterial("M_Food_NasiPutih", "M_Food_NasiPutih_Albedo", new Vector2(2f, 2f));
            CreateMaterial("M_Food_Sayur", "M_Food_Sayur_Albedo", new Vector2(3f, 3f));
            AssetDatabase.SaveAssets();
        }

        private static void CreateMaterial(string materialName, string textureName, Vector2 tiling)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureFolder}/{textureName}.png");
            if (shader == null || texture == null)
            {
                Debug.LogError($"[FoodAssets] Missing shader or texture for {materialName}.");
                return;
            }

            string path = $"{MaterialFolder}/{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetTexture("_BaseMap", texture);
            material.SetTexture("_MainTex", texture);
            material.SetTextureScale("_BaseMap", tiling);
            material.SetTextureScale("_MainTex", tiling);
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_Color", Color.white);
            material.SetFloat("_Smoothness", 0.15f);
            material.SetFloat("_Glossiness", 0.15f);
            EditorUtility.SetDirty(material);
        }

        private static void UpdateModelPrefabs()
        {
            // Rebuilt meshes keep the Blender Z-up authoring, so the Model child needs
            // the same 270° X rotation the imported FBX root uses.
            ApplyMesh("ayam_goreng", "Food_ayam_goreng", "M_Food_Ayam");
            ApplyMesh("kerupuk", "Food_kerupuk", "M_Food_Kerupuk");
            ApplyMesh("nasi_putih", "Food_nasi_putih", "M_Food_NasiPutih");
            ApplyMesh("sayur_singkong", "Food_sayur_singkong", "M_Food_Sayur", 3);
        }

        private static void ApplyMesh(string prefabName, string meshAssetName, string materialName, int materialSlots = 1)
        {
            string prefabPath = $"{PrefabFolder}/{prefabName}.prefab";
            var mesh = LoadMesh($"{ModelFolder}/{meshAssetName}.fbx");
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/{materialName}.mat");

            if (mesh == null || material == null)
            {
                Debug.LogError($"[FoodAssets] Missing mesh or material for {prefabName} (mesh={meshAssetName}, material={materialName}).");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var model = root.transform.Find("Model");
                if (model == null)
                {
                    Debug.LogError($"[FoodAssets] {prefabName} has no Model child.");
                    return;
                }

                var meshFilter = model.GetComponent<MeshFilter>();
                if (meshFilter == null)
                    meshFilter = model.gameObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = mesh;

                model.localPosition = Vector3.zero;
                model.localRotation = Quaternion.Euler(270f, 0f, 0f);
                model.localScale = Vector3.one;

                var renderer = model.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var materials = new Material[Mathf.Max(1, materialSlots)];
                    for (int i = 0; i < materials.Length; i++)
                        materials[i] = material;
                    renderer.sharedMaterials = materials;
                }

                RefitCollider(root, model, mesh);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[FoodAssets] {prefabName}: mesh '{mesh.name}' ({mesh.vertexCount} verts, {mesh.subMeshCount} submesh) applied.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RefitCollider(GameObject root, Transform model, Mesh mesh)
        {
            var collider = root.GetComponent<BoxCollider>();
            if (collider == null)
                collider = root.AddComponent<BoxCollider>();

            var bounds = mesh.bounds;
            var center = model.TransformPoint(bounds.center);
            var extents = model.rotation * Vector3.Scale(bounds.extents, model.lossyScale);

            var size = new Vector3(Mathf.Abs(extents.x), Mathf.Abs(extents.y), Mathf.Abs(extents.z)) * 2f;
            collider.center = root.transform.InverseTransformPoint(center);
            collider.size = size;
        }

        private static Mesh LoadMesh(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                return null;

            var filter = prefab.GetComponentInChildren<MeshFilter>(true);
            return filter != null ? filter.sharedMesh : null;
        }

        private static void WriteTextureIfMissing(string name, System.Func<Color32[]> generator)
        {
            string path = $"{TextureFolder}/{name}.png";
            if (File.Exists(path))
                return;

            const int size = 512;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(generator());
            texture.Apply();

            Directory.CreateDirectory(TextureFolder);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.maxTextureSize = 512;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }

            Debug.Log($"[FoodAssets] Generated {path}.");
        }

        // -----------------------------------------------------------------
        // Procedural albedos (tileable, no external references needed)
        // -----------------------------------------------------------------
        private const int Size = 512;

        private static Color32[] GenerateCrackerTexture()
        {
            var pixels = new Color32[Size * Size];
            var baseColor = new Color(0.90f, 0.80f, 0.58f);
            var dark = new Color(0.58f, 0.42f, 0.20f);
            var light = new Color(0.99f, 0.93f, 0.78f);

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float u = x / (float)Size;
                    float v = y / (float)Size;
                    float speck = ValueNoise(u, v, 64, 11);
                    Color color = Color.Lerp(baseColor, light, speck * 0.7f);

                    float pore = Hash(x, y, Size, 23);
                    if (pore > 0.90f)
                        color = Color.Lerp(color, dark, 0.6f);
                    else if (pore < 0.05f)
                        color = Color.Lerp(color, light, 0.65f);

                    float blister = ValueNoise(u, v, 12, 31);
                    if (blister > 0.80f)
                        color = Color.Lerp(color, light, (blister - 0.80f) * 3f);

                    pixels[y * Size + x] = color;
                }
            }

            return pixels;
        }

        private static Color32[] GenerateRiceTexture()
        {
            var pixels = new Color32[Size * Size];
            var baseColor = new Color(0.95f, 0.935f, 0.90f);
            var shadow = new Color(0.72f, 0.70f, 0.64f);
            var highlight = new Color(1f, 0.995f, 0.965f);

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float u = x / (float)Size;
                    float v = y / (float)Size;

                    // Elongated grains: strong contrast so the icon does not read as a blank oval.
                    float grain = ValueNoise(u * 2.3f, v * 5.2f, 56, 7);
                    Color color = Color.Lerp(baseColor, highlight, Mathf.Clamp01(grain * 1.35f - 0.1f));

                    float shadowGrain = ValueNoise(u * 2.1f + 0.37f, v * 4.9f + 0.61f, 56, 19);
                    if (shadowGrain < 0.42f)
                        color = Color.Lerp(color, shadow, (0.42f - shadowGrain) * 1.2f);

                    float speck = Hash(x, y, Size, 41);
                    if (speck > 0.97f)
                        color = Color.Lerp(color, shadow, 0.55f);

                    pixels[y * Size + x] = color;
                }
            }

            return pixels;
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
