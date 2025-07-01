using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private List<SceneButton> sceneButtons;
    
    private List<UIButton> _uiButtons = new();

    private void Awake()
    {
        for (int i = 0; i < sceneButtons.Count; i++)
        {
            var uiButton = new UIButton(sceneButtons[i].button, sceneButtons[i].sceneName);
            uiButton.Subscribe(HandleLevelSelected);
            _uiButtons.Add(uiButton);
        }
    }
    
    private void HandleLevelSelected(string sceneName)
    {
            SceneManager.LoadScene(sceneName);
    }
}