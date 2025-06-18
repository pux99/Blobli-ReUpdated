using System;
using System.Collections.Generic;
using Enemies.Fungi;
using Enemies.LightBug;
using Grid;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Utilities
{
    public class GameManager : MonoBehaviour
    {
        private GridManager _gridManager;
        public static event Action OnStepTaken;
        private void Awake() => ServiceLocator.Instance.RegisterService(this);

        [SerializeField] private int stepCounter;

        void Start()
        {
            _gridManager = ServiceLocator.Instance.GetService<GridManager>();
            SetUpFungi();
            SetUpLightBug();
        }
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


        #region Fungi
            [Header("Fungi")]
            [SerializeField] private FungiLevelSO fungiInLevel;
            [SerializeField] private GameObject[] fungiGameObjects;
            private List<Fungi> _fungiInstances = new();
            
            private void SetUpFungi()
            {
                if (fungiGameObjects.Length == 0) return;
                
                for (int i = 0; i < fungiGameObjects.Length; i++)
                {
                    var fungi = new Fungi(
                        fungiGameObjects[i],
                        _gridManager,
                        fungiInLevel.RandomLightTile(),
                        fungiInLevel.offSprite,
                        fungiInLevel.onSprite,
                        fungiInLevel.onToOffList[i],
                        fungiInLevel.offToOnList[i]
                    );
                    OnStepTaken += fungi.OnStepTaken;
                    _fungiInstances.Add(fungi);
                }
            }
        #endregion

        #region LightBug
            [Header("LightBug")]
            [SerializeField] private LightBugGenericSO lightBugGenericSo;
            [SerializeField] private LightBugLevelSO lightBugInLevel;
            [SerializeField] private GameObject[] lightBugGameObjects;
            
            private readonly List<LightBug> _lightBugs = new();

            private void SetUpLightBug()
            {
                for (int i = 0; i < lightBugGameObjects.Length; i++)
                {
                    var bug = new LightBug(
                        lightBugGameObjects[i],
                        lightBugGenericSo,
                        _gridManager,
                        lightBugInLevel.bugIntensities[i],
                        lightBugInLevel.bugSpeeds[i]
                    );

                    _lightBugs.Add(bug);
                    OnStepTaken += bug.OnStepTaken;
                }
            }

        #endregion
    }
}
