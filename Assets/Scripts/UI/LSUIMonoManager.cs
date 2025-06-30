using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class LSUIMonoManager : MonoBehaviour
{
    [SerializeField] private List<Button> levelButtons;

    private List<UIButton> _uiButtons = new();

    public Button buttonToMainMenu;
    public Button buttonToLevelSelector;

    private void Awake()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            var uiButton = new UIButton(levelButtons[i], i + 1);
            uiButton.Subscribe(HandleLevelSelected);
            _uiButtons.Add(uiButton);
        }
        buttonToMainMenu?.onClick.AddListener(ToMainMenu);
        buttonToLevelSelector?.onClick.AddListener(ToLevelSelector);
    }

    private void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    private void ToLevelSelector()
    {
        SceneManager.LoadScene("LevelSelector");
    }

    private void HandleLevelSelected(int level)
    {
        string sceneName = level switch
        {
            1 => "Tutorial",
            2 => "Level2",
            3 => "Level3",
            4 => "Level4",
            5 => "Level5",
            _ => null
        };

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.Log($"Nivel {level} aún no desbloqueado.");
    }
}
