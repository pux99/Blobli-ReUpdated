using System.Collections;
using UnityEngine;
using Utilities.MonoManager;
using UnityEngine.SceneManagement;
using Player;

public class Altar : MonoBehaviour, IUpdatable
{
    private Inventory _inventory;
    private Transform _playerTransform;
    [SerializeField] private float interactionRange = 1.0f;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;
    [SerializeField] PlayerController playerController;
    
    private void Awake()
    {
        ServiceLocator.Instance.RegisterService(this);
    }
    
    private void Start()
    {
        _playerTransform =  playerController.transform;
        
        var updateManager = ServiceLocator.Instance.GetService<CustomMonoManager>();
        updateManager?.RegisterOnUpdate(this);

        StartCoroutine(delay());
    }

    private IEnumerator delay()
    {
        yield return new WaitForEndOfFrame();
        _inventory =  playerController.Inventory;
    }
    
    public void Tick(float deltaTime)
    {
        if (_playerTransform == null || _inventory == null) return;

        if (Input.GetKeyDown(interactionKey))
        {
            if (!(Vector2.Distance(transform.position, _playerTransform.position) <= interactionRange)) return;
            if (_inventory.CheckKeys())
            {
                SceneManager.LoadScene("VictoryScene");
            }
        }
    }
}