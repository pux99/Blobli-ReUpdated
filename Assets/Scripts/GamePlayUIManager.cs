using System;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities;

public class GamePlayUIManager : MonoBehaviour
{
    public UiInventory Inventory;
    public PauseMenu PauseMenu;
    void Start()
    {
        Inventory.Start(this);
        PauseMenu.Setup();
    }

    public void ToggelPauseMenu()
    {
        PauseMenu.pauseMenu.SetActive(!PauseMenu.pauseMenu.activeInHierarchy);
    }
}
[Serializable]
public class PauseMenu
{
    public GameObject pauseMenu;
    public Button resume;
    public Button reset;
    public Button maimMenu;
    public int menuLevel;

    public void Setup()
    {
        resume.onClick.AddListener(TurnOff);
        reset.onClick.AddListener(ResetLevel);
        maimMenu.onClick.AddListener(()=>SceneManager.LoadScene(menuLevel));

    }

    public void ResetLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    private void TurnOff()
    {
        pauseMenu.SetActive(false);
    }
}
