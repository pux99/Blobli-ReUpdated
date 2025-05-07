using Grid;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Utilities;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class Fungi : MonoBehaviour
    {
        [Header("Possible LightTiles")]
        [SerializeField] private TileBase[] tileVariants;
    
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite offSprite;
        [SerializeField] private Sprite onSprite;
        
        [Header("Fungi Configuration")]
        [SerializeField] private int onToOff = 2;
        [SerializeField] private int offToOn = 2;
    
        private bool _isOn = false;
        private int _stepsSinceLastChange = 0;
        private Vector3Int _cellPos;
    
        private GridManager _gridManager;
        private void OnEnable()
        {
            GameManager.OnStepTaken += OnStepTaken;
        }

        private void OnDisable()
        {
            GameManager.OnStepTaken -= OnStepTaken;
        }
        private void Start()
        {
            _gridManager = ServiceLocator.Instance.GetService<GridManager>();
        
            _cellPos = _gridManager.PositionInGrid(transform.position);
        
            UpdateState();
        }

        private void OnStepTaken()
        {
            _stepsSinceLastChange++;

            switch (_isOn)
            {
                case false when _stepsSinceLastChange >= offToOn:
                    _isOn = true;
                    _stepsSinceLastChange = 0;
                    UpdateState();
                    break;
            
                case true when _stepsSinceLastChange >= onToOff:
                    _isOn = false;
                    _stepsSinceLastChange = 0;
                    UpdateState();
                    break;
            }
        }

        private void UpdateState()
        {
            if (!_gridManager.IsShadowTile(_cellPos)){spriteRenderer.sprite = offSprite; return;}
        
            var map = _gridManager.TileMaps.light;
        
            if (_isOn)
            {
                spriteRenderer.sprite = onSprite;
                TileBase randomTile = tileVariants[Random.Range(0, tileVariants.Length)];
                map.SetTile(_cellPos, randomTile);
            }
            else
            {
                spriteRenderer.sprite = offSprite;
                map.SetTile(_cellPos, null);
            }
        }
    }
}
