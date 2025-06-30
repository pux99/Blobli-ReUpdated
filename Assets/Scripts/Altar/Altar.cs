using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.MonoManager;

    [Serializable]
    public class Altar : IUpdatable, IStartable
    {
        private Inventory _inventory;
        private Transform _playerTransform;
        [SerializeField] private float interactionRange = 1.0f;
        [SerializeField] private KeyCode interactionKey = KeyCode.F;
        [SerializeField] PlayerController playerController;
        private CustomMonoManager _customMonoManager;
        
        private MonoBehaviour _monoBehaviour;
        [SerializeField] public GameObject altar;
    
        public void Awake(MonoBehaviour mono)
        {
            ServiceLocator.Instance.RegisterService(this);
            _customMonoManager = ServiceLocator.Instance.GetService<CustomMonoManager>();
            _customMonoManager.RegisterOnStart(this);
            _monoBehaviour = mono;
        }
    
        public void Beginning()
        {
            _playerTransform =  playerController.transform;
            _customMonoManager.RegisterOnUpdate(this);
            _monoBehaviour.StartCoroutine(delay());
        }

        private IEnumerator delay()
        {
            yield return new WaitForEndOfFrame();
            _inventory =  playerController.Inventory;
        }
        public void Tick(float deltaTime)
        {
            if (!_playerTransform || _inventory == null) return;

            if (!Input.GetKeyDown(interactionKey)) return;
            if (!(Vector2.Distance(altar.transform.position, _playerTransform.position) <= interactionRange)) return;
            if (_inventory.CheckKeys())
            {
                SceneManager.LoadScene("VictoryScene");
            }
        }
    }