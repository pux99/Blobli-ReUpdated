using UnityEngine.SceneManagement;

namespace Utilities
{
    public static class ResetScript
    {
        public static void ResetLevel()
        {
            if (ServiceLocator.Instance != null) ServiceLocator.Instance.ClearServices();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
