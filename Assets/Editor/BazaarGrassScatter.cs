using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NutriPick.Editor
{
    /// <summary>
    /// Scatters low-poly grass tufts over the bazaar lawns.
    /// Meshes are generated procedurally, everything is merged into a single static
    /// mesh (one draw call) and the paved areas are excluded.
    ///
    /// Idempotent: rebuilding replaces the previous GrassField root and mesh asset.
    /// </summary>
    public static class BazaarGrassScatter
    {
        private const string ScenePath = "Assets/Scenes/Level03_Bazaar.unity";
        private const string RootName = "GrassField";
        private const string MeshAssetPath = "Assets/Models/Environment/Bazaar/BazaarGrassField.asset";
        private const string MaterialPath = "Assets/Materials/Bazaar/M_Bazaar_GrassTuft.mat";

        private const int TargetTufts = 400;
        private const float GridStep = 2.6f;
        private const float Jitter = 1.05f;
        private const float AreaMinX = -55f;
        private const float AreaMaxX = 55f;
        private const float AreaMinZ = -60f;
        private const float AreaMaxZ = 60f;
        private const int Seed = 20261123;

        private static readonly string[] PavedNames =
        {
            "Road_Flamboyant", "Sidewalk_West", "Sidewalk_East", "Plaza_Center", "Forecourt_Gate",
            "Campus_Ground", "Pramuka_Road", "Pramuka_Sidewalk", "Pramuka_Apron", "SMK_Ground",
            "Curb_West", "Curb_East", "Curb_Pramuka"
        };

        private static readonly string[] PropPrefixes =
        {
            "BazaarStand_", "DecorStall_Closed_", "PanggungBazaar", "Cafe_", "SchoolCampus"
        };

        [MenuItem("NutriPick/Bazaar/Build Grass Field")]
        public static void Build()
        {
            EnsureScene();

            var scene = SceneManager.GetActiveScene();
            var sceneRoot = GameObject.Find("Scene3Bazaar");
            if (sceneRoot == null)
            {
                Debug.LogError("[BazaarGrass] Missing Scene3Bazaar root.");
                return;
            }

            var previous = sceneRoot.transform.Find(RootName);
            if (previous != null)
                Object.DestroyImmediate(previous.gameObject);

            var material = GetOrCreateMaterial();
            var variants = new[]
            {
                BuildTuft(6, Seed + 1),
                BuildTuft(8, Seed + 2),
                BuildTuft(10, Seed + 3)
            };

            var exclusions = CollectExclusions();
            var random = new System.Random(Seed);
            var candidates = new List<Vector3>();

            for (float x = AreaMinX; x <= AreaMaxX; x += GridStep)
            {
                for (float z = AreaMinZ; z <= AreaMaxZ; z += GridStep)
                {
                    float px = x + Range(random, -Jitter, Jitter);
                    float pz = z + Range(random, -Jitter, Jitter);

                    if (InsideExclusion(exclusions, px, pz))
                        continue;

                    // The lawns are the only ground surface with a collider; anything else
                    // (campus, stands, props) is either excluded or blocks the ray.
                    var origin = new Vector3(px, 2f, pz);
                    if (!Physics.Raycast(origin, Vector3.down, out var hit, 5f))
                        continue;
                    if (hit.collider == null || hit.collider.name != "Ground_Base")
                        continue;

                    candidates.Add(new Vector3(px, 0f, pz));
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[BazaarGrass] No valid grass spots found.");
                return;
            }

            // Deterministic shuffle, then keep the target count.
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int count = Mathf.Min(TargetTufts, candidates.Count);
            var combines = new List<CombineInstance>(count * 3);
            for (int i = 0; i < count; i++)
            {
                // Each spot gets a small clump so the lawn reads as grass without
                // increasing the draw-call count (still one merged mesh).
                int clumpSize = 3;
                for (int c = 0; c < clumpSize; c++)
                {
                    var offset = new Vector3(Range(random, -0.22f, 0.22f), 0f, Range(random, -0.22f, 0.22f));
                    var transform = Matrix4x4.TRS(
                        candidates[i] + offset,
                        Quaternion.Euler(0f, Range(random, 0f, 360f), 0f),
                        Vector3.one * Range(random, 0.9f, 1.4f));

                    combines.Add(new CombineInstance
                    {
                        mesh = variants[(i + c) % variants.Length],
                        transform = transform
                    });
                }
            }

            var combined = new Mesh { name = "BazaarGrassField", indexFormat = IndexFormat.UInt32 };
            combined.CombineMeshes(combines.ToArray(), true, true);

            Directory.CreateDirectory(Path.GetDirectoryName(MeshAssetPath) ?? "Assets");
            AssetDatabase.DeleteAsset(MeshAssetPath);
            AssetDatabase.CreateAsset(combined, MeshAssetPath);

            var field = new GameObject(RootName);
            field.transform.SetParent(sceneRoot.transform, false);
            field.AddComponent<MeshFilter>().sharedMesh = combined;
            var renderer = field.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            GameObjectUtility.SetStaticEditorFlags(field, StaticEditorFlags.BatchingStatic);

            foreach (var variant in variants)
                Object.DestroyImmediate(variant);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[BazaarGrass] Placed {count} tufts ({(combined.triangles.Length / 3)} tris) as one static mesh.");
        }

        private static Material GetOrCreateMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            var color = new Color(0.30f, 0.50f, 0.22f);
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            material.SetFloat("_Cull", 0f); // double sided blades
            material.SetFloat("_Smoothness", 0.1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static List<Rect> CollectExclusions()
        {
            var rects = new List<Rect>();

            foreach (var name in PavedNames)
            {
                var go = GameObject.Find(name);
                if (go != null)
                    AddBounds(rects, go, 0.25f);
            }

            foreach (var renderer in Object.FindObjectsByType<Renderer>())
            {
                var t = renderer.transform;
                while (t != null)
                {
                    if (StartsWithAny(t.name, PropPrefixes))
                    {
                        AddBounds(rects, t.gameObject, 0.5f);
                        break;
                    }

                    t = t.parent;
                }
            }

            // Player spawn should stay clear.
            rects.Add(Rect.MinMaxRect(-13f, 38f, -5f, 45f));
            return rects;
        }

        private static void AddBounds(List<Rect> rects, GameObject go, float margin)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            rects.Add(Rect.MinMaxRect(
                bounds.min.x - margin, bounds.min.z - margin,
                bounds.max.x + margin, bounds.max.z + margin));
        }

        private static bool StartsWithAny(string value, string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (value.StartsWith(prefix))
                    return true;
            }

            return false;
        }

        private static bool InsideExclusion(List<Rect> rects, float x, float z)
        {
            foreach (var rect in rects)
            {
                if (rect.Contains(new Vector2(x, z)))
                    return true;
            }

            return false;
        }

        private static Mesh BuildTuft(int bladeCount, int seed)
        {
            var random = new System.Random(seed);
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (int blade = 0; blade < bladeCount; blade++)
            {
                float angle = (blade / (float)bladeCount) * Mathf.PI * 2f + Range(random, -0.35f, 0.35f);
                float lean = Range(random, 0.10f, 0.30f);
                float height = Range(random, 0.20f, 0.40f);
                float width = Range(random, 0.012f, 0.020f);

                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                var side = new Vector3(-direction.z, 0f, direction.x);
                var basePosition = direction * Range(random, 0f, 0.035f);
                var mid = basePosition + direction * (lean * 0.55f) + Vector3.up * (height * 0.55f);
                var tip = basePosition + direction * lean + Vector3.up * height;

                int start = vertices.Count;
                vertices.Add(basePosition - side * width);
                vertices.Add(basePosition + side * width);
                vertices.Add(mid - side * width * 0.6f);
                vertices.Add(mid + side * width * 0.6f);
                vertices.Add(tip - side * width * 0.15f);
                vertices.Add(tip + side * width * 0.15f);

                triangles.AddRange(new[] { start, start + 1, start + 3, start, start + 3, start + 2 });
                triangles.AddRange(new[] { start + 2, start + 3, start + 5, start + 2, start + 5, start + 4 });
            }

            var mesh = new Mesh { name = "GrassTuft" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private static float Range(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static void EnsureScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
