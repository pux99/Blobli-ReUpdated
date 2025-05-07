using System.Collections.Generic;
using System.Linq;
using Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Potions
{
    public class Potion : MonoBehaviour
    {
        [Header("Drawing Shape")]
        [Tooltip("Temporary")]
        [SerializeField] private SO_ShadowShape shape; //This should not be here, it should be set by the Inventory
    
        #region Private var
    
        private List<Vector3Int> _markedPositions = new(); //List that shows where the shadow would be placed
        private GridManager _gridManager;
        private Tilemap _shadowMap; //Map in which it will paint
        private Vector3 _playerDir;

        #endregion
        public bool ShadowIndicator { get; private set; } = false;


        private void Start()
        {
            _gridManager = ServiceLocator.Instance.GetService<GridManager>();
            _shadowMap = _gridManager.TileMaps.shadow; //Has its color set to black and opacity to 200.
        }
        
        public void UsePotion(Vector3 dir)
        {
            _playerDir = dir;
            if (!ShadowIndicator)
            {
                ShowIndicator();
            }
            else
            {
                ThrowPotion();
            }
        }
        public void HideIndicator()
        {
            ShadowIndicator = false;
            ClearIndicator();
        }
        public void SetShape(SO_ShadowShape newShape) //The inventory should set the shape of the potion
        {
            shape = newShape;
        }
        
        private void ShowIndicator() //Shows where the shadow would be placed
        {
            ClearIndicator();
            ShadowIndicator = true;
            
            Vector3Int playerPos = _gridManager.PlayerTile();
            Vector3Int direction = Vector3Int.RoundToInt(_playerDir);
            Vector3Int forward = playerPos + direction;
            
            _markedPositions = shape.relativePositions.Select(rel => RotateDirection(rel, direction)).Select(rotated => forward + new Vector3Int(rotated.x, rotated.y, 0)).ToList();

            var canPlaceAll = _markedPositions.All(pos => _gridManager.CanPlaceShadow(pos));
            var indicatorColor = canPlaceAll ? new Color(0, 0, 0, 200f / 255f) : new Color(1, 0, 0, 200f / 255f);
            _shadowMap.color = indicatorColor;

            foreach (var pos in _markedPositions)
            {
                _shadowMap.SetTile(pos, _gridManager.ShadowTile);
            }
        }
        public void ShowIndicator(Vector3 dir) //Shows where the shadow would be placed
        {
            ClearIndicator();
            ShadowIndicator = true;
            
            Vector3Int playerPos = _gridManager.PlayerTile();
            Vector3Int direction = Vector3Int.RoundToInt(dir);
            Vector3Int forward = playerPos + direction;
            
            _markedPositions = shape.relativePositions.Select(rel => RotateDirection(rel, direction)).Select(rotated => forward + new Vector3Int(rotated.x, rotated.y, 0)).ToList();

            var canPlaceAll = _markedPositions.All(pos => _gridManager.CanPlaceShadow(pos));
            var indicatorColor = canPlaceAll ? new Color(0, 0, 0, 200f / 255f) : new Color(1, 0, 0, 200f / 255f);
            _shadowMap.color = indicatorColor;

            foreach (var pos in _markedPositions)
            {
                _shadowMap.SetTile(pos, _gridManager.ShadowTile);
            }
        }
        private void ThrowPotion() //Sets shadow on the marker positions
        {
            foreach (var pos in _markedPositions.Where(pos => _gridManager.CanPlaceShadow(pos)))
            {
                _gridManager.TileMaps.light.SetTile(pos, _gridManager.ShadowTile);
            }

            ClearIndicator();
        }
        private void ClearIndicator() //Resets the marked positions
        {
            foreach (var pos in _markedPositions)
            {
                _shadowMap.SetTile(pos, null);
            }
            
            _markedPositions.Clear();
        }
        private Vector2Int RotateDirection(Vector2Int pos, Vector3Int dir)
        {
            if (dir == Vector3Int.up) return pos; // Up (0, 1) → 0°
            if (dir == Vector3Int.down) return new Vector2Int(-pos.x, -pos.y); // Down (0, -1) → 180°
            if (dir == Vector3Int.right) return new Vector2Int(pos.y, -pos.x); // Right (1, 0) → 90°
            if (dir == Vector3Int.left) return new Vector2Int(-pos.y, pos.x); // Left (-1, 0) → -90°
            return pos; // No rotation
        }
    }
}
