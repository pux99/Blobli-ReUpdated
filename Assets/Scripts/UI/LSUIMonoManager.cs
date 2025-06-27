using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class LSUIMonoManager : MonoBehaviour
{
    [SerializeField] private List<Button> levelButtons;

    private List<UIButton> _uiButtons = new();

    private void Awake()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            var uiButton = new UIButton(levelButtons[i], i + 1);
            uiButton.Subscribe(HandleLevelSelected);
            _uiButtons.Add(uiButton);
        }
    }

    private void HandleLevelSelected(int level)
    {
        if (level == 1)
            SceneManager.LoadScene("Tutorial");
        else
            Debug.Log($"Nivel {level} aún no desbloqueado.");
    }
}
