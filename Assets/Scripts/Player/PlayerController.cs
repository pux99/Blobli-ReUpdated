using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    private InputAction _moveAction;
    private bool _isMoving = false;
    
    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
    }
    
    public void Move()
    {
        if (_isMoving) return;

        Vector2 input = _moveAction.ReadValue<Vector2>();

        Vector2 moveTo = Vector2.zero;
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            moveTo = input.x > 0 ? Vector2.right : Vector2.left;
        else if (Mathf.Abs(input.y) > 0)
            moveTo = input.y > 0 ? Vector2.up : Vector2.down;

        if (moveTo == Vector2.zero)
            return;

        if (!gridManager.CanMove(moveTo)) return;

        _isMoving = true;

        transform.position += new Vector3(moveTo.x, moveTo.y, 0);
        
        Invoke(nameof(ResetMove), 0.1f);
    }

    private void ResetMove()
    {
        _isMoving = false;
    }
}
