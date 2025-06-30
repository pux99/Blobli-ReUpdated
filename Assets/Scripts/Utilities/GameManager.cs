using System;
using System.Collections.Generic;
using Enemies.Fungi;
using Enemies.LightBug;
using GemScripts;
using Grid;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Utilities
{
    public class GameManager : MonoBehaviour
    {
        [Header("Enemies generic data")] [SerializeField]
        private EnemySO genericData;

        [Header("Fungus in level")] [SerializeField]
        private List<FungiStats> fungus;

        [Header("LightBugs in leve")] [SerializeField]
        private List<LightBugStats> lightBugs;

        public event Action OnStepTaken;
        private int stepCounter;

        private void Awake()
        {
            ServiceLocator.Instance.RegisterService(this);
            if (gemManager.gemContainer != null)
            {
                gemManager.Awake();
            }
        }

    void Start()
        {
            StartLevel();
        }
        private void StartLevel()
        {
            if (fungus != null)
            {
                SetUpFungi();
            }
            if (lightBugs != null)
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
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        public void Defeat()
        {
            SceneManager.LoadScene("DefeatScene");
        }


        #region Fungi
            private readonly List<Fungi> spawnedFungi = new();
            private void SetUpFungi()
            {
                foreach (var stat in fungus)
                {
                    Fungi fungi = new Fungi(
                        stat,
                        genericData
                    );
                    OnStepTaken += fungi.OnStepTaken;
                    spawnedFungi.Add(fungi);
                }
            }
            
        #endregion
        
        #region LightBug
            private readonly List<LightBug> spawnedLightBugs = new();
            private void SetUpLightBugs()
            {
                foreach (var stat in lightBugs)
                {
                    LightBug lightBug = new LightBug(
                        stat,
                        genericData
                    );
                    OnStepTaken += lightBug.OnStepTaken;
                    spawnedLightBugs.Add(lightBug);
                }
            }

            private void OnDrawGizmosSelected()
            {
                foreach (var bug in lightBugs)
                {
                    Vector3 currentPos = bug.lightBugGameObject.transform.position;
                    Gizmos.color = Color.black;
                    Gizmos.DrawSphere(currentPos, 0.2f);
                    
                    for (int i = 0; i < bug.directions.Count; i++)
                    {
                        Vector3 nextPos = currentPos + bug.directions[i];
                        
                        Gizmos.DrawLine(currentPos, nextPos);

                        currentPos = nextPos;
                        
                        Gizmos.DrawSphere(currentPos, 0.2f);
                    }
                }
            }

            #endregion

            #region Gems

            public GemManager gemManager;

            #endregion
    }
}
