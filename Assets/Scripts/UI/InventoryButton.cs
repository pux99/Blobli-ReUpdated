using UnityEngine;
using UnityEngine.Serialization;
using Utilities.MonoManager;

public class InventoryButton :MonoBehaviour,IUpdatable, IStartable
{
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private GameObject inventorySprite;
    [SerializeField] private CustomMonoManager _customMonoManager;

    private Vector3 _inventorySpriteSp;
    private float _duration;
    private float _timer;
    [SerializeField]private bool _open;
    [SerializeField]private bool _move;

    private void Awake()
    {
        _customMonoManager.RegisterOnStart(this);
    }
    public void Beginning()
    {
        _inventorySpriteSp = inventorySprite.transform.localPosition;
        if (_curve.length > 0)
            _duration = _curve.keys[_curve.length - 1].time;
        ServiceLocator.Instance.GetService<CustomMonoManager>().RegisterOnUpdate(this);
    }

    public void Tick(float deltaTime)
    {
        if (_move)
        {
            _timer += deltaTime;
            if (_timer<_duration)
            {
                if(_open)
                {
                    inventorySprite.transform.localPosition =
                        _inventorySpriteSp - new Vector3(_curve.Evaluate(_timer), 0);
                }
                else
                {
                    inventorySprite.transform.localPosition =_inventorySpriteSp+new Vector3(_curve.Evaluate(_timer),0);
                }
            }
            else
            {
                _move = false;
            }
        }
    }
    [ContextMenu("MOVE")]
    public void StartMovement()
    {
        if(!_move)
        {
            _move = true;
            _open = !_open;
            _timer = 0;
            _inventorySpriteSp = inventorySprite.transform.localPosition;
        }
    }
}
