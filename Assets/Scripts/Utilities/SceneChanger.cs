using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string nextScene;
    
    public void ChangeScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
