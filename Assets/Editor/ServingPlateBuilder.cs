using UnityEditor;
using UnityEngine;
using NutriPick.Interaction;

namespace NutriPick.Editor
{
    public static class ServingPlateBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/Interaction/ServingPlate.prefab";
        private const string PlateModelPath = "Assets/Models/Props/ceramic_plate.fbx";
        private const string HighlightMaterialPath = "Assets/Materials/SlotHighlight.mat";

        [MenuItem("NutriPick/Build Serving Plate")]
        public static void Build()
        {
            var plateModel = AssetDatabase.LoadAssetAtPath<GameObject>(PlateModelPath);
            if (plateModel == null)
            {
                Debug.LogError($"[ServingPlate] Missing {PlateModelPath}");
                return;
            }

            var root = new GameObject("ServingPlate");
            var controller = root.AddComponent<PlateController>();
            controller.minItems = 4;
            controller.requiresDrink = false;

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(plateModel, root.transform);
            visual.name = "PlateVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            var highlightMaterial = AssetDatabase.LoadAssetAtPath<Material>(HighlightMaterialPath);
            var positions = new[]
            {
                new Vector3(0.06f, 0.02f, 0.06f),
                new Vector3(-0.06f, 0.02f, 0.06f),
                new Vector3(0.06f, 0.02f, -0.06f),
                new Vector3(-0.06f, 0.02f, -0.06f)
            };

            var slots = new PlateSlot[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                var slotObject = new GameObject("Slot" + (i + 1));
                slotObject.transform.SetParent(root.transform, false);
                slotObject.transform.localPosition = positions[i];

                var slot = slotObject.AddComponent<PlateSlot>();
                var trigger = slotObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(0.08f, 0.04f, 0.08f);
                trigger.center = new Vector3(0f, 0.02f, 0f);

                var snap = new GameObject("Snap");
                snap.transform.SetParent(slotObject.transform, false);
                snap.transform.localPosition = new Vector3(0f, 0.03f, 0f);

                var highlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                highlight.name = "Highlight";
                highlight.transform.SetParent(slotObject.transform, false);
                highlight.transform.localPosition = new Vector3(0f, 0.006f, 0f);
                highlight.transform.localScale = new Vector3(0.09f, 0.001f, 0.09f);
                var highlightCollider = highlight.GetComponent<Collider>();
                if (highlightCollider != null)
                    Object.DestroyImmediate(highlightCollider);
                if (highlightMaterial != null)
                    highlight.GetComponent<Renderer>().sharedMaterial = highlightMaterial;
                highlight.SetActive(false);

                slot.plate = controller;
                slot.snapPoint = snap.transform;
                slot.trigger = trigger;
                slot.highlight = highlight;
                slots[i] = slot;
            }

            controller.slots = slots;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ServingPlate] Created {PrefabPath}");
        }
    }
}
