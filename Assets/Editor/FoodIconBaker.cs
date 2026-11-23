using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NutriPick.Nutrition;

namespace NutriPick.Editor
{
    /// <summary>
    /// Renders a coloured inventory icon for every FoodData prefab.
    /// Uses PreviewRenderUtility so the open scene is never touched
    /// (the old generator replaced the scene and produced white silhouettes).
    /// </summary>
    public static class FoodIconBaker
    {
        private const string IconFolder = "Assets/Textures/FoodIcons";
        private const string FoodDataFolder = "Assets/ScriptableObjects/Food";
        private const int IconSize = 256;

        [MenuItem("NutriPick/Food/Bake Food Icons")]
        public static void BakeAll()
        {
            Directory.CreateDirectory(IconFolder);

            var guids = AssetDatabase.FindAssets("t:FoodData", new[] { FoodDataFolder });
            int baked = 0;

            foreach (var guid in guids.OrderBy(g => g))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var food = AssetDatabase.LoadAssetAtPath<FoodData>(path);
                if (food == null || food.prefab == null)
                {
                    Debug.LogWarning($"[FoodIcons] Skipping {path} (no prefab).");
                    continue;
                }

                var iconPath = $"{IconFolder}/{food.id}.png";
                if (Bake(food.prefab, iconPath))
                {
                    var sprite = ConfigureSprite(iconPath);
                    if (sprite != null)
                    {
                        food.icon = sprite;
                        EditorUtility.SetDirty(food);
                        baked++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FoodIcons] Baked {baked} icons.");
        }

        private static bool Bake(GameObject prefab, string path)
        {
            var preview = new PreviewRenderUtility();
            GameObject instance = null;
            try
            {
                preview.camera.fieldOfView = 30f;
                preview.camera.nearClipPlane = 0.01f;
                preview.camera.farClipPlane = 50f;
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                preview.camera.allowHDR = false;
                preview.camera.allowMSAA = false;

                foreach (var light in preview.lights)
                {
                    light.intensity = 1.3f;
                    light.transform.rotation = Quaternion.Euler(35f, 35f, 0f);
                }

                instance = Object.Instantiate(prefab);
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                preview.AddSingleGO(instance);
                var renderers = instance.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                {
                    Debug.LogWarning($"[FoodIcons] {prefab.name} has no renderer.");
                    return false;
                }

                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                float radius = Mathf.Max(0.0001f, bounds.extents.magnitude);
                float distance = radius / Mathf.Tan(preview.camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.2f;
                var direction = new Vector3(0.6f, 0.85f, -1f).normalized;

                preview.camera.transform.position = bounds.center + direction * distance;
                preview.camera.transform.LookAt(bounds.center);

                preview.BeginPreview(new Rect(0, 0, IconSize, IconSize), GUIStyle.none);
                preview.camera.Render();
                var rendered = preview.EndPreview();

                var target = RenderTexture.GetTemporary(IconSize, IconSize, 24, RenderTextureFormat.ARGB32);
                var previous = RenderTexture.active;
                try
                {
                    Graphics.Blit(rendered, target);
                    RenderTexture.active = target;

                    var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
                    try
                    {
                        texture.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
                        texture.Apply();
                        File.WriteAllBytes(path, texture.EncodeToPNG());
                    }
                    finally
                    {
                        Object.DestroyImmediate(texture);
                    }
                }
                finally
                {
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(target);
                }

                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[FoodIcons] Failed to bake {prefab.name}: {exception.Message}");
                return false;
            }
            finally
            {
                if (instance != null)
                    Object.DestroyImmediate(instance);
                preview.Cleanup();
            }
        }

        private static Sprite ConfigureSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return null;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = IconSize;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
