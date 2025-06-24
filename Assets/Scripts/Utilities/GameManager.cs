using System;
using System.Collections.Generic;
using Enemies.Fungi;
using Enemies.LightBug;
using Grid;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Utilities
{
    public class GameManager : MonoBehaviour
    {
        private int stepCounter;
        public static event Action OnStepTaken;
        private void Awake() => ServiceLocator.Instance.RegisterService(this);
        
        void Start()
        {
            StartLevel();
        }
        private void StartLevel()
        {
            if (fungiData != null)
            {
                SetUpFungi();
            }
            if (lightBugData != null)
            {
                SetUpLightBugs();
            }
        }
        public void Step()
        {
            stepCounter++;
            OnStepTaken?.Invoke();
        }
        public void ResetLevel(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled) return;
            
            ServiceLocator.Instance.ClearServices();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        public void Defeat()
        {
            SceneManager.LoadScene("DefeatScene");
        }


        #region Fungi
            [Header("Fungi")]
            [SerializeField] private FungiLevelSO fungiData;
            private readonly List<Fungi> _spawnedFungi = new();
            
            private void SetUpFungi()
            {
                foreach (var stat in fungiData.fungi)
                {
                    var fungiGO = Instantiate(stat.fungiGameObject, (Vector3Int)stat.position, Quaternion.identity, transform);
                    
                    Fungi fungi = new Fungi(
                        fungiGO,
                        fungiData.offSprite,
                        fungiData.onSprite,
                        stat.onToOff,
                        stat.offToOn
                    );
                    OnStepTaken += fungi.OnStepTaken;
                    _spawnedFungi.Add(fungi);
                }
            }

            private void ClearFungi()
            {
                _spawnedFungi.Clear();
            }
            
        #endregion
        
        #region LightBug
            [Header("LightBug")]
            [SerializeField] private LightBugLevelSO lightBugData;
            private readonly List<LightBug> _spawnedLightBugs = new();
                
            private void SetUpLightBugs()
            {
                foreach (var stat in lightBugData.lightBugs)
                {
                    var lightBugGo = Instantiate(stat.lightBugGameObject, (Vector3Int)stat.initialPosition, Quaternion.identity, transform);
                        
                    LightBug lightBug = new LightBug(
                        lightBugGo, 
                        stat,
                        lightBugData.animationFrames
                    );
                    OnStepTaken += lightBug.OnStepTaken;
                    _spawnedLightBugs.Add(lightBug);
                }
            }

            private void ClearLightBugs()
            {
                _spawnedLightBugs.Clear();
            }
            
        #endregion
    }
}
