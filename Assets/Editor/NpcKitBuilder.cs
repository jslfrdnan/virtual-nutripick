using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using NutriPick.NPC;

namespace NutriPick.Editor
{
    public static class NpcKitBuilder
    {
        private const string ModelFolder = "Assets/Models/Characters/NPC";
        private const string MaterialFolder = "Assets/Materials/NPC";
        private const string PrefabFolder = "Assets/Prefabs/NPC";
        private const string ControllerFolder = "Assets/Animations/NPC";
        private const string LayerName = "NPC";

        private static readonly string[] StateNames = { "Idle", "Walk", "Eat", "Talk", "Wipe" };

        private class MatSpec
        {
            public Color color;
            public float smoothness;
            public float metallic;
        }

        private static readonly Dictionary<string, MatSpec> Palette = new()
        {
            { "M_NPC_SkinA", new MatSpec { color = new Color(0.72f, 0.50f, 0.34f), smoothness = 0.28f } },
            { "M_NPC_SkinB", new MatSpec { color = new Color(0.54f, 0.34f, 0.20f), smoothness = 0.30f } },
            { "M_NPC_SkinC", new MatSpec { color = new Color(0.85f, 0.66f, 0.50f), smoothness = 0.26f } },
            { "M_NPC_Hair", new MatSpec { color = new Color(0.045f, 0.035f, 0.030f), smoothness = 0.18f } },
            { "M_NPC_Eye", new MatSpec { color = new Color(0.02f, 0.02f, 0.02f), smoothness = 0.55f } },
            { "M_NPC_Mouth", new MatSpec { color = new Color(0.32f, 0.12f, 0.10f), smoothness = 0.25f } },
            { "M_NPC_ShirtTeal", new MatSpec { color = new Color(0.055f, 0.30f, 0.30f), smoothness = 0.12f } },
            { "M_NPC_ShirtRust", new MatSpec { color = new Color(0.52f, 0.16f, 0.10f), smoothness = 0.12f } },
            { "M_NPC_ShirtCream", new MatSpec { color = new Color(0.82f, 0.74f, 0.60f), smoothness = 0.12f } },
            { "M_NPC_ShirtGreen", new MatSpec { color = new Color(0.16f, 0.30f, 0.18f), smoothness = 0.12f } },
            { "M_NPC_ShirtBlue", new MatSpec { color = new Color(0.14f, 0.24f, 0.40f), smoothness = 0.12f } },
            { "M_NPC_Pants", new MatSpec { color = new Color(0.10f, 0.12f, 0.15f), smoothness = 0.10f } },
            { "M_NPC_PantsKhaki", new MatSpec { color = new Color(0.40f, 0.34f, 0.24f), smoothness = 0.10f } },
            { "M_NPC_PantsDenim", new MatSpec { color = new Color(0.16f, 0.20f, 0.28f), smoothness = 0.10f } },
            { "M_NPC_Apron", new MatSpec { color = new Color(0.86f, 0.82f, 0.70f), smoothness = 0.10f } },
            { "M_NPC_Hijab", new MatSpec { color = new Color(0.30f, 0.42f, 0.42f), smoothness = 0.08f } },
            { "M_NPC_HijabCream", new MatSpec { color = new Color(0.78f, 0.70f, 0.56f), smoothness = 0.08f } },
            { "M_NPC_HijabRose", new MatSpec { color = new Color(0.58f, 0.28f, 0.30f), smoothness = 0.08f } },
            { "M_NPC_Shoe", new MatSpec { color = new Color(0.055f, 0.045f, 0.04f), smoothness = 0.16f } },
            { "M_NPC_Cap", new MatSpec { color = new Color(0.12f, 0.18f, 0.15f), smoothness = 0.10f } },
            { "M_NPC_Belt", new MatSpec { color = new Color(0.06f, 0.05f, 0.04f), smoothness = 0.32f } },
        };

        private class NpcVariant
        {
            public string model;
            public string prefabName;
            public string shirt;
            public string pants;
            public string hijab;
        }

        private static readonly NpcVariant[] Variants =
        {
            new() { model = "Npc_Male", prefabName = "NPC_Male_A", shirt = "M_NPC_ShirtTeal", pants = "M_NPC_Pants" },
            new() { model = "Npc_Male", prefabName = "NPC_Male_B", shirt = "M_NPC_ShirtRust", pants = "M_NPC_PantsKhaki" },
            new() { model = "Npc_Female", prefabName = "NPC_Female_A", shirt = "M_NPC_ShirtRust", pants = "M_NPC_PantsKhaki" },
            new() { model = "Npc_Female", prefabName = "NPC_Female_B", shirt = "M_NPC_ShirtBlue", pants = "M_NPC_PantsDenim" },
            new() { model = "Npc_FemaleHijab", prefabName = "NPC_FemaleHijab_A", shirt = "M_NPC_ShirtCream", pants = "M_NPC_Pants", hijab = "M_NPC_Hijab" },
            new() { model = "Npc_FemaleHijab", prefabName = "NPC_FemaleHijab_B", shirt = "M_NPC_ShirtGreen", pants = "M_NPC_PantsKhaki", hijab = "M_NPC_HijabCream" },
            new() { model = "Npc_FemaleHijab", prefabName = "NPC_FemaleHijab_C", shirt = "M_NPC_ShirtCream", pants = "M_NPC_PantsKhaki", hijab = "M_NPC_HijabRose" },
        };

        private static readonly Dictionary<string, Material> Materials = new();
        private static readonly HashSet<string> MissingMaterials = new();

        [MenuItem("NutriPick/Build NPC Kit")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            EnsureFolders();
            EnsureLayer(LayerName);
            ConfigureImporters();
            EnsureMaterials();

            foreach (var variant in Variants)
                CreatePrefab(variant);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[NpcKit] Built {Variants.Length} NPC prefabs with five clips each.");
        }

        public static void ConfigureRole(GameObject instance, bool apron, bool cap)
        {
            if (instance == null)
                return;

            SetMeshActive(instance.transform, "_Apron", apron);
            SetMeshActive(instance.transform, "_Cap", cap);
        }

        private static void SetMeshActive(Transform root, string suffix, bool active)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name.EndsWith(suffix, System.StringComparison.Ordinal))
                    transform.gameObject.SetActive(active);
            }
        }

        private static void EnsureFolders()
        {
            foreach (var folder in new[] { MaterialFolder, PrefabFolder, ControllerFolder })
                EnsureFolder(folder);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
                return;

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void EnsureLayer(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) >= 0)
                return;

            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[NpcKit] TagManager.asset not found; add the NPC layer manually.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            var layers = tagManager.FindProperty("layers");
            if (layers == null)
                return;

            for (int i = 8; i < layers.arraySize; i++)
            {
                var entry = layers.GetArrayElementAtIndex(i);
                if (!string.IsNullOrEmpty(entry.stringValue))
                    continue;

                entry.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log($"[NpcKit] Created layer '{layerName}' at index {i}.");
                return;
            }

            Debug.LogWarning("[NpcKit] No free user layer for NPC.");
        }

        private static void ConfigureImporters()
        {
            foreach (var model in Variants.Select(variant => variant.model).Distinct())
            {
                var path = $"{ModelFolder}/{model}.fbx";
                if (!File.Exists(path))
                {
                    Debug.LogError($"[NpcKit] Missing model: {path}");
                    continue;
                }

                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null)
                    continue;

                importer.globalScale = 1f;
                importer.useFileScale = true;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importBlendShapes = false;
                importer.importAnimation = true;
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
                importer.animationCompression = ModelImporterAnimationCompression.Optimal;
                importer.animationRotationError = 0.5f;
                importer.animationPositionError = 0.5f;
                importer.meshCompression = ModelImporterMeshCompression.Off;
                importer.optimizeBones = false;
                importer.isReadable = false;
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation = ModelImporterMaterialLocation.InPrefab;

                var clips = importer.defaultClipAnimations;
                foreach (var clip in clips)
                {
                    clip.loopTime = true;
                    clip.loopPose = true;
                }

                if (clips.Length > 0)
                    importer.clipAnimations = clips;

                importer.SaveAndReimport();
            }
        }

        private static void EnsureMaterials()
        {
            var litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null)
            {
                Debug.LogError("[NpcKit] URP Lit shader not found.");
                return;
            }

            foreach (var pair in Palette)
            {
                var path = $"{MaterialFolder}/{pair.Key}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(litShader);
                    AssetDatabase.CreateAsset(material, path);
                }

                material.shader = litShader;
                material.SetColor("_BaseColor", pair.Value.color);
                material.SetFloat("_Metallic", pair.Value.metallic);
                material.SetFloat("_Smoothness", pair.Value.smoothness);
                EditorUtility.SetDirty(material);
                Materials[pair.Key] = material;
            }

            AssetDatabase.SaveAssets();
        }

        private static void CreatePrefab(NpcVariant variant)
        {
            var fbxPath = $"{ModelFolder}/{variant.model}.fbx";
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (model == null)
            {
                Debug.LogError($"[NpcKit] Could not load {fbxPath}");
                return;
            }

            var controller = CreateController(variant.model, fbxPath);
            var root = Object.Instantiate(model);
            root.name = variant.prefabName;
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            root.layer = LayerMask.NameToLayer(LayerName);

            RemapMaterials(root, variant);

            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var npcAnimator = root.AddComponent<NpcAnimator>();
            npcAnimator.animator = animator;
            npcAnimator.defaultState = "Idle";

            var marker = root.AddComponent<NpcMarker>();
            marker.role = "Visitor";

            var walker = root.AddComponent<NpcWalker>();
            walker.animator = npcAnimator;

            var body = root.AddComponent<CapsuleCollider>();
            var modelHeight = variant.model == "Npc_Male" ? 1.72f : 1.64f;
            body.center = new Vector3(0f, modelHeight * 0.5f, 0f);
            body.height = modelHeight;
            body.radius = 0.28f;
            body.direction = 1;

            var rigidbody = root.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            SetMeshActive(root.transform, "_Apron", false);
            SetMeshActive(root.transform, "_Cap", false);

            var prefabPath = $"{PrefabFolder}/{variant.prefabName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
        }

        private static AnimatorController CreateController(string model, string fbxPath)
        {
            var controllerPath = $"{ControllerFolder}/AC_{model}.controller";
            AssetDatabase.DeleteAsset(controllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = null;

            foreach (var stateName in StateNames)
            {
                var clip = LoadClip(fbxPath, stateName);
                if (clip == null)
                {
                    Debug.LogError($"[NpcKit] Missing clip '{stateName}' in {fbxPath}");
                    continue;
                }

                var state = stateMachine.AddState(stateName);
                state.motion = clip;
                state.speed = 1f;
                state.writeDefaultValues = true;

                if (stateName == "Idle")
                    idleState = state;
            }

            if (idleState != null)
                stateMachine.defaultState = idleState;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimationClip LoadClip(string fbxPath, string clipName)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                if (asset is AnimationClip clip && clip.name == clipName)
                    return clip;
            }

            return null;
        }

        private static void RemapMaterials(GameObject root, NpcVariant variant)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int index = 0; index < materials.Length; index++)
                {
                    if (materials[index] == null)
                        continue;

                    var key = NormalizeMaterialName(materials[index].name);
                    if (key.StartsWith("M_NPC_Shirt") && !string.IsNullOrEmpty(variant.shirt))
                        key = variant.shirt;
                    else if (key.StartsWith("M_NPC_Pants") && !string.IsNullOrEmpty(variant.pants))
                        key = variant.pants;
                    else if (key.StartsWith("M_NPC_Hijab") && !string.IsNullOrEmpty(variant.hijab))
                        key = variant.hijab;

                    if (Materials.TryGetValue(key, out var replacement))
                    {
                        materials[index] = replacement;
                    }
                    else if (MissingMaterials.Add(key))
                    {
                        Debug.LogWarning($"[NpcKit] No URP material for '{key}' on {renderer.name}.");
                    }
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static string NormalizeMaterialName(string name)
        {
            return name.Replace(" (Instance)", string.Empty).Split('.')[0].Trim();
        }
    }
}
