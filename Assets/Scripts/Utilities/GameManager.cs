using System;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Utilities
{
    public class GameManager : MonoBehaviour
    {
        public static event Action OnStepTaken;
        private void Awake() => ServiceLocator.Instance.RegisterService(this);

        [SerializeField] private int stepCounter;
        
        public void Step()
        {
            stepCounter++;
            OnStepTaken?.Invoke();
        }
        public void ResetLevel(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled) return;
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        public void Defeat()
        {
            SceneManager.LoadScene("DefeatScene");
        }
    }
}
