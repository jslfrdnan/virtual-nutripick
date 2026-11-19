using UnityEngine;
using UnityEngine.SceneManagement;

namespace NutriPick.Flow
{
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"[NutriPick] Scene '{sceneName}' is not in the build settings. Staying in the current scene.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
