using System;
using System.Collections.Generic;
using Enemies.Fungi;
using Enemies.LightBug;
using Grid;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
            [SerializeField] private FungiSO fungiSo;
            [SerializeField] private GameObject[] fungiGameObjects;
            [SerializeField] private int[] onToOffList;
            [SerializeField] private int[] offToOnList;

            
            private List<Fungi> _fungiInstances = new();
            
            private void SetUpFungi()
            {
                if (fungiGameObjects.Length == 0) return;
                for (int i = 0; i < fungiGameObjects.Length; i++)
                {
                    var fungi = new Fungi(
                        fungiGameObjects[i],
                        _gridManager,
                        fungiSo.RandomLightTile(),
                        fungiSo.offSprite,
                        fungiSo.onSprite,
                        onToOffList[i],
                        offToOnList[i]
                    );
                    OnStepTaken += fungi.OnStepTaken;
                    _fungiInstances.Add(fungi);
                }
            }
        #endregion

        #region LightBug
            [Header("LightBug")]
            [SerializeField] private LightBugSO lightBugSo;
            [SerializeField] private GameObject[] lightBugGameObjects;
            
            //[SerializeField] private List<Vector3Int>[] bugPaths;
            [SerializeField] private int[] bugIntensities;
            [SerializeField] private int[] bugSpeeds;

            private readonly List<LightBug> _lightBugs = new();

            private void SetUpLightBug()
            {
                for (int i = 0; i < lightBugGameObjects.Length; i++)
                {
                    var bug = new LightBug(
                        lightBugGameObjects[i],
                        lightBugSo,
                        _gridManager,
                        bugIntensities[i],
                        bugSpeeds[i]
                    );

                    _lightBugs.Add(bug);
                    OnStepTaken += bug.OnStepTaken;
                }
            }

        #endregion
    }
}
