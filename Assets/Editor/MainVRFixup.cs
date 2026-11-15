using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NutriPick.Editor
{
    public static class MainVRFixup
    {
        [MenuItem("NutriPick/Fix MainVR Table Surface")]
        public static void FixTableSurface()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainVR.unity");

            if (GameObject.Find("TableSurfaceCollider") == null)
            {
                var tableSurface = new GameObject("TableSurfaceCollider");
                tableSurface.transform.position = new Vector3(-0.01f, 1.305f, 0.01f);
                var box = tableSurface.AddComponent<BoxCollider>();
                box.size = new Vector3(2.0f, 0.02f, 1.88f);
            }

            var stand = GameObject.Find("TableFoodStand");
            if (stand != null)
                stand.SetActive(false);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainVR.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("[NutriPick] MainVR table surface collider is ready.");
        }
    }
}
