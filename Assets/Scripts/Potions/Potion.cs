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

        #endregion
        public bool Marking { get; private set; } = false;
    
        private void Start()
        {
            _gridManager = ServiceLocator.Instance.GetService<GridManager>();
            _shadowMap = _gridManager.TileMaps.shadow; //Has its color set to black and opacity to 200.
        }
        public void ShowMarker(Vector3 dir) //Shows where the shadow would be placed
        {
            ResetMarker();
            Marking = true;
        
            Vector3Int playerPos = _gridManager.PlayerTile();
            Vector3Int direction = Vector3Int.RoundToInt(dir);
            Vector3Int forward = playerPos + direction;

            foreach (var rel in shape.relativePositions)
            {
                Vector2Int rotated = RotateDirection(rel, direction);
                Vector3Int pos = forward + new Vector3Int(rotated.x, rotated.y, 0);
                _shadowMap.SetTile(pos, _gridManager.ShadowTile);
                _markedPositions.Add(pos);
            }
        }
        public void UsePotion() //Sets shadow on the marker positions
        {
            foreach (var pos in _markedPositions.Where(pos => _gridManager.CanPlaceShadow(pos)))
            {
                _gridManager.TileMaps.light.SetTile(pos, _gridManager.ShadowTile);
            }

            ResetMarker();
        }
        public void SetShape(SO_ShadowShape newShape) //The inventory should set the shape of the potion
        {
            shape = newShape;
        }
        private void ResetMarker() //Resets the marked positions
        {
            foreach (var pos in _markedPositions)
            {
                _shadowMap.SetTile(pos, null);
            }

            Marking = false;
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
