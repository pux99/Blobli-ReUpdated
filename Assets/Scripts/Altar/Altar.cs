using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.MonoManager;

namespace Altar
{
    public class Altar : MonoBehaviour, IUpdatable, IStartable
    {
        private Inventory _inventory;
        private Transform _playerTransform;
        [SerializeField] private float interactionRange = 1.0f;
        [SerializeField] private KeyCode interactionKey = KeyCode.F;
        [SerializeField] PlayerController playerController;
        private CustomMonoManager _customMonoManager;
    
        private void Awake()
        {
            ServiceLocator.Instance.RegisterService(this);
            _customMonoManager = ServiceLocator.Instance.GetService<CustomMonoManager>();
            _customMonoManager.RegisterOnStart(this);
        }
    
        public void Beginning()
        {
            _playerTransform =  playerController.transform;
            _customMonoManager.RegisterOnUpdate(this);
            StartCoroutine(delay());
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
            if (!(Vector2.Distance(transform.position, _playerTransform.position) <= interactionRange)) return;
            if (_inventory.CheckKeys())
            {
                SceneManager.LoadScene("VictoryScene");
            }
        }
    }
}