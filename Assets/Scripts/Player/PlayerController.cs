using Grid;
using Potions;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
using Utilities.UpdateManager;

namespace Player
{
    public class PlayerController : MonoBehaviour, IUpdatable
    {
        [SerializeField] private float speed;
        
        private InputAction _moveAction;
        private bool _isMoving;
        private bool _isRotating;
        private Vector3 _dir;
        private Vector3 _lastDir;
        private Vector3 _startingPos;
        private Quaternion _angle;
        
        private GameManager _gameManager;
        private GridManager _gridManager;

        public Potion currentPotion;

        private void Update() //To test the potion.
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                currentPotion.ShowIndicator(_lastDir);
            }            
            if (Input.GetKeyDown(KeyCode.K))
            {
                currentPotion.UsePotion();
            }
        }

        private void Awake()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _startingPos = transform.position;
        }
        //Preguntarle al profe si se puede cambiar el orden de ejecucion
        //para que se ejecute primero el customupdate manager y para poder usar el awake y no el start
        private void Start()
        { 
            ServiceLocator.Instance.GetService<CustomUpdateManager>().Register(this);
            _gameManager = ServiceLocator.Instance.GetService<GameManager>();
            _gridManager = ServiceLocator.Instance.GetService<GridManager>();
        } 

        public void Move()
        {
            if (_isMoving) return;
            
            _dir = _moveAction.ReadValue<Vector2>();
            if (_dir == Vector3.zero) return;
            if (_dir != _lastDir)
            {
                LookDir();
                return;
            }
            if (!_gridManager.CanMove(_dir)||_isRotating) return;
            _startingPos = transform.position;
            _isMoving = true;
        }

        public void Tick(float deltaTime)
        {
            IsDead();
            if(_isMoving)
            {
                transform.position = Vector3.MoveTowards(transform.position, _startingPos + _dir, speed * deltaTime);
                if (Vector3.Distance(transform.position, _startingPos + _dir) < 0.1)
                    ResetMove();
            }
            if (_isRotating)
            {
                transform.rotation=Quaternion.RotateTowards(transform.rotation, _angle, speed*200 * deltaTime);
                if (Quaternion.Angle(transform.rotation,  _angle) < 0.1f)
                    ResetRotation();
            }
        }

        private void LookDir()
        {
            var angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
            var offset=90;
            if (((Vector2)_lastDir == Vector2.right && (Vector2)_dir == Vector2.left) || ((Vector2)_lastDir == Vector2.up && (Vector2)_dir == Vector2.down))//ver como hacer para cambiar el sentido de giro(no la pregunta esta bien pero no la conseciencia
            {
                offset = 90+360;
            }
            _angle = Quaternion.Euler(0f, 0f, angle - offset);
            _lastDir = _dir;
            _isRotating = true;
        }

        private void ResetRotation()
        {
            if (currentPotion.ShadowIndicator) currentPotion.ShowIndicator(_lastDir);
            _isRotating = false;
            transform.rotation = _angle;
        }

        private void ResetMove()
        {
            currentPotion.HideIndicator();
            _isMoving = false;
            transform.position = _startingPos + _dir;
            AddStep();
        }

        private void IsDead()
        {
            if (_gridManager.IsInLight()) gameObject.SetActive(false);
        }

        void AddStep()
        {
            _gameManager.Step();
        }
    }
}
