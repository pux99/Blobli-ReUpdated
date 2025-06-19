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
        private GridManager _gridManager;
        public static event Action OnStepTaken;
        private void Awake() => ServiceLocator.Instance.RegisterService(this);

        [SerializeField] private int stepCounter;

        void Start()
        {
            _gridManager = ServiceLocator.Instance.GetService<GridManager>();
            StartLevel();
        }

        private void StartLevel()
        {
            SetUpFungi();
            SetUpLightBugs();
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
            [SerializeField] private FungiLevelSO fungiData;
            private readonly List<Fungi> _spawnedFungi = new();
            
            private void SetUpFungi()
            {
                foreach (var stat in fungiData.fungi)
                {
                    var fungiGO = Instantiate(stat.fungiGameObject, (Vector3Int)stat.position, Quaternion.identity);
                    TileBase tile = fungiData.tileVariants[UnityEngine.Random.Range(0, fungiData.tileVariants.Length)];
                    
                    Fungi fungi = new Fungi(
                        fungiGO,
                        _gridManager,
                        tile,
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
                var lightBugGo = Instantiate(stat.lightBugGameObject, (Vector3Int)stat.initialPosition, Quaternion.identity);
                TileBase tile = lightBugData.tileVariants[UnityEngine.Random.Range(0, lightBugData.tileVariants.Length)];
                    
                LightBug lightBug = new LightBug(
                    lightBugGo, 
                    _gridManager,
                    stat.path,
                    stat.speed,
                    stat.lightIntensity,
                    lightBugData,
                    stat.initialPosition
                );
                OnStepTaken += lightBug.OnStepTaken;
                _spawnedLightBugs.Add(lightBug);
            }
        }

        private void ClearLightBugs()
        {
            _spawnedFungi.Clear();
        }
            
        #endregion
    }
}
