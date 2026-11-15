using UnityEngine.SceneManagement;

namespace NutriPick.Flow
{
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            SceneManager.LoadScene(sceneName);
        }
    }
}
