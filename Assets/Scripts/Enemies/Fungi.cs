using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Fungi : MonoBehaviour
{
    [SerializeField] private Tilemap lightTileMap; //Hacer una mejor manera de poder acceder a esto, todos los enemigo van a tener que acceder a 1 o mas tilemaps
    [SerializeField] private TileBase[] tileVariants;
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;
    
    [SerializeField] private int OnToOff = 2;
    [SerializeField] private int OffToOn = 2;
    
    private bool _isOn = false;
    private int _stepsSinceLastChange = 0;
    private Vector3Int _cellPos;
    
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
        _cellPos = lightTileMap.WorldToCell(transform.position);
        
        UpdateState();
    }

    private void OnStepTaken()
    {
        _stepsSinceLastChange++;

        switch (_isOn)
        {
            case false when _stepsSinceLastChange >= OffToOn:
                _isOn = true;
                _stepsSinceLastChange = 0;
                UpdateState();
                break;
            
            case true when _stepsSinceLastChange >= OnToOff:
                _isOn = false;
                _stepsSinceLastChange = 0;
                UpdateState();
                break;
        }
    }

    private void UpdateState()
    {
        if (_isOn)
        {
            spriteRenderer.sprite = onSprite;
            TileBase randomTile = tileVariants[Random.Range(0, tileVariants.Length)];
            lightTileMap.SetTile(_cellPos, randomTile);
        }
        else
        {
            spriteRenderer.sprite = offSprite;
            lightTileMap.SetTile(_cellPos, null);
        }
    }
}
