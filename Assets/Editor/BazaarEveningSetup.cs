using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NutriPick.Editor
{
    /// <summary>
    /// Turns Level 3 into a late-afternoon bazaar (golden hour).
    /// Only touches RenderSettings, the skybox material and the light rig.
    /// </summary>
    public static class BazaarEveningSetup
    {
        private const string ScenePath = "Assets/Scenes/Level03_Bazaar.unity";
        private const string SkyMaterialPath = "Assets/Materials/Bazaar/M_Bazaar_Sky.mat";

        [MenuItem("NutriPick/Bazaar/Apply Golden Hour")]
        public static void ApplyGoldenHour()
        {
            EnsureScene();

            var sun = GameObject.Find("Sun");
            var sunLight = sun != null ? sun.GetComponent<Light>() : null;
            if (sunLight != null)
            {
                sunLight.transform.rotation = Quaternion.Euler(16f, 305f, 0f);
                sunLight.color = new Color(1.00f, 0.68f, 0.42f);
                sunLight.intensity = 1.0f;
                sunLight.shadows = LightShadows.Soft;
                sunLight.shadowStrength = 0.85f;
            }
            else
            {
                Debug.LogWarning("[BazaarEvening] Sun light not found.");
            }

            TuneFill("Fill_CafeNorth", 0.85f);
            TuneFill("Fill_CafeSouth", 0.85f);
            TuneFill("Fill_Stage", 0.95f);

            var sky = AssetDatabase.LoadAssetAtPath<Material>(SkyMaterialPath);
            if (sky != null)
            {
                RenderSettings.skybox = sky;
                sky.SetColor("_SkyTint", new Color(0.62f, 0.44f, 0.34f));
                sky.SetColor("_GroundColor", new Color(0.22f, 0.17f, 0.15f));
                sky.SetFloat("_AtmosphereThickness", 1.45f);
                sky.SetFloat("_Exposure", 0.95f);
                sky.SetFloat("_SunSize", 0.07f);
                sky.SetFloat("_SunSizeConvergence", 6f);
                EditorUtility.SetDirty(sky);
            }
            else
            {
                Debug.LogWarning($"[BazaarEvening] Missing sky material at {SkyMaterialPath}.");
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.40f, 0.32f, 0.30f);
            RenderSettings.ambientEquatorColor = new Color(0.32f, 0.25f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.12f, 0.10f);
            RenderSettings.ambientIntensity = 1f;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.62f, 0.45f, 0.36f);
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 130f;

            RenderSettings.sun = sunLight;

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log("[BazaarEvening] Golden hour applied to Level 3.");
        }

        private static void TuneFill(string name, float intensity)
        {
            var go = GameObject.Find(name);
            var light = go != null ? go.GetComponent<Light>() : null;
            if (light == null)
                return;

            light.color = new Color(1.00f, 0.74f, 0.48f);
            light.intensity = intensity;
        }

        private static void EnsureScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
                EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
