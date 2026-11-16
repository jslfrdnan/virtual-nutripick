using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using NutriPick.Nutrition;

namespace NutriPick.Editor
{
    public static class FoodIconGenerator
    {
        private const string IconFolder = "Assets/Textures/FoodIcons";
        private const string PlaceholderPath = IconFolder + "/PlaceholderFood.png";
        private static readonly string[] FoodIds =
        {
            "rendang",
            "burger",
            "telur_rebus",
            "telur_ceplok",
            "tempe",
            "brokoli",
            "pisang",
            "mie_goreng"
        };

        [MenuItem("NutriPick/Generate Scene 1 Food Icons")]
        public static void Generate()
        {
            if (!Application.isBatchMode)
            {
                Debug.LogError("[FoodIcon] Run this generator in batchmode; it replaces the open scene with a temporary render scene.");
                return;
            }

            Directory.CreateDirectory(IconFolder);
            AssetDatabase.Refresh();
            EnsureSpriteImporter(PlaceholderPath);

            int generated = 0;

            foreach (var id in FoodIds)
            {
                var foodPath = $"Assets/ScriptableObjects/Food/FD_{id}.asset";
                var food = AssetDatabase.LoadAssetAtPath<FoodData>(foodPath);
                if (food == null)
                {
                    Debug.LogError($"[FoodIcon] Missing {foodPath}");
                    continue;
                }

                var prefabPath = AssetDatabase.GetAssetPath(food.prefab);
                var sprite = RenderPrefab(prefabPath, id);

                // The temporary render scene unloads loaded assets, so reload after rendering.
                food = AssetDatabase.LoadAssetAtPath<FoodData>(foodPath);
                var placeholder = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderPath);
                if (food == null)
                    continue;

                if (sprite != null)
                {
                    food.icon = sprite;
                    EditorUtility.SetDirty(food);
                    generated++;
                }
                else if (placeholder != null)
                {
                    food.icon = placeholder;
                    EditorUtility.SetDirty(food);
                    Debug.LogWarning($"[FoodIcon] Using placeholder for {id}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FoodIcon] Generated {generated} food icons.");
        }

        private static Sprite RenderPrefab(string prefabPath, string id)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject instance = null;
            Camera camera = null;
            GameObject lightObject = null;
            RenderTexture renderTexture = null;
            Texture2D texture = null;

            try
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogError($"[FoodIcon] Missing prefab {prefabPath}");
                    return null;
                }

                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                instance.SetActive(true);

                var renderers = instance.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                    return null;

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);
                Debug.Log($"[FoodIcon] {id} bounds center={bounds.center} size={bounds.size} renderers={renderers.Length}");

                var cameraObject = new GameObject("IconCamera");
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<UniversalAdditionalCameraData>();
                camera.enabled = true;
                camera.cameraType = CameraType.Game;
                camera.useOcclusionCulling = false;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.fieldOfView = 30f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 100f;
                camera.cullingMask = ~0;

                float radius = Mathf.Max(0.001f, bounds.extents.magnitude);
                float distance = radius / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.25f;
                var direction = new Vector3(0.6f, 0.8f, -1f).normalized;
                camera.transform.position = bounds.center + direction * distance;
                camera.transform.LookAt(bounds.center);
                Debug.Log($"[FoodIcon] {id} camera={camera.transform.position} target={bounds.center}");

                lightObject = new GameObject("IconLight");
                SceneManager.MoveGameObjectToScene(lightObject, scene);
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.4f;
                light.transform.rotation = Quaternion.Euler(35f, 35f, 0f);

                renderTexture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
                renderTexture.Create();
                camera.targetTexture = renderTexture;
                camera.Render();

                RenderTexture.active = renderTexture;
                texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                texture.Apply();
                RenderTexture.active = null;
                camera.targetTexture = null;

                var path = $"{IconFolder}/{id}.png";
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                EnsureSpriteImporter(path);
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[FoodIcon] Failed to render {id}: {exception.Message}");
                return null;
            }
            finally
            {
                if (renderTexture != null)
                {
                    renderTexture.Release();
                    Object.DestroyImmediate(renderTexture);
                }
                if (texture != null)
                    Object.DestroyImmediate(texture);
                if (camera != null)
                    Object.DestroyImmediate(camera.gameObject);
                if (lightObject != null)
                    Object.DestroyImmediate(lightObject);
                if (instance != null)
                    Object.DestroyImmediate(instance);
            }
        }

        private static void EnsureSpriteImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }
    }
}
