using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NutriPick.Editor
{
    /// <summary>
    /// Adds simple low-poly street lamps along the bazaar curbs and switches them on
    /// (emissive head + a warm, shadowless point light). Idempotent: rebuilding replaces
    /// the previous LampPosts root.
    /// </summary>
    public static class BazaarStreetLamps
    {
        private const string ScenePath = "Assets/Scenes/Level03_Bazaar.unity";
        private const string RootName = "LampPosts";
        private const string MetalMaterialPath = "Assets/Materials/Bazaar/M_Bazaar_LampMetal.mat";
        private const string GlowMaterialPath = "Assets/Materials/Bazaar/M_Bazaar_LampGlow.mat";

        // x is the curb line, z the spacing; armDirection +1 points to +X, -1 to -X.
        private static readonly Vector3[] WestLamps =
        {
            new Vector3(-12.55f, 0f, 34f),
            new Vector3(-12.55f, 0f, 12f),
            new Vector3(-12.55f, 0f, -12f),
            new Vector3(-12.55f, 0f, -36f)
        };

        private static readonly Vector3[] EastLamps =
        {
            new Vector3(-5.45f, 0f, 24f),
            new Vector3(-5.45f, 0f, 2f),
            new Vector3(-5.45f, 0f, -24f)
        };

        [MenuItem("NutriPick/Bazaar/Build Street Lamps")]
        public static void Build()
        {
            EnsureScene();

            var scene = SceneManager.GetActiveScene();
            var sceneRoot = GameObject.Find("Scene3Bazaar");
            if (sceneRoot == null)
            {
                Debug.LogError("[BazaarLamps] Missing Scene3Bazaar root.");
                return;
            }

            var previous = sceneRoot.transform.Find(RootName);
            if (previous != null)
                Object.DestroyImmediate(previous.gameObject);

            var metal = GetOrCreateMaterial(MetalMaterialPath, "Universal Render Pipeline/Lit",
                new Color(0.13f, 0.13f, 0.14f), 0.45f);
            var glow = GetOrCreateMaterial(GlowMaterialPath, "Universal Render Pipeline/Unlit",
                new Color(1.00f, 0.78f, 0.45f), 0.15f);

            var root = new GameObject(RootName);
            root.transform.SetParent(sceneRoot.transform, false);

            int index = 0;
            foreach (var position in WestLamps)
                CreateLamp(root.transform, position, 0f, metal, glow, index++);
            foreach (var position in EastLamps)
                CreateLamp(root.transform, position, 180f, metal, glow, index++);

            GameObjectUtility.SetStaticEditorFlags(root, StaticEditorFlags.BatchingStatic);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[BazaarLamps] Built {index} street lamps.");
        }

        private static void CreateLamp(Transform parent, Vector3 position, float yaw, Material metal, Material glow, int index)
        {
            var lamp = new GameObject($"LampPost_{index:00}");
            lamp.transform.SetParent(parent, false);
            lamp.transform.position = position;
            lamp.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            var pole = AddPrimitive(lamp.transform, PrimitiveType.Cylinder, "Pole", metal,
                new Vector3(0f, 2.0f, 0f), new Vector3(0.07f, 2.0f, 0.07f));

            var arm = AddPrimitive(lamp.transform, PrimitiveType.Cube, "Arm", metal,
                new Vector3(0.26f, 3.92f, 0f), new Vector3(0.55f, 0.06f, 0.06f));

            var head = AddPrimitive(lamp.transform, PrimitiveType.Sphere, "Head", glow,
                new Vector3(0.52f, 3.82f, 0f), new Vector3(0.20f, 0.13f, 0.20f));

            var lightObject = new GameObject("Light");
            lightObject.transform.SetParent(lamp.transform, false);
            lightObject.transform.localPosition = new Vector3(0.52f, 3.78f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.00f, 0.78f, 0.50f);
            light.intensity = 1.2f;
            light.range = 7f;
            light.shadows = LightShadows.None;

            foreach (var part in new[] { pole, arm, head })
                GameObjectUtility.SetStaticEditorFlags(part, StaticEditorFlags.BatchingStatic);
        }

        private static GameObject AddPrimitive(Transform parent, PrimitiveType type, string name, Material material, Vector3 localPosition, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;

            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            return go;
        }

        private static Material GetOrCreateMaterial(string path, string shaderName, Color color, float smoothness)
        {
            var shader = Shader.Find(shaderName);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
