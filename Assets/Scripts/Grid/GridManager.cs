using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] public UnityEngine.Grid grid;
        [Header("Tiles")] 
        [SerializeField] private TileBase shadowTile;
        public TileBase ShadowTile => shadowTile;
        
        [System.Serializable]
        public class TileMapGroup
        {
            public Tilemap border;
            public Tilemap floor;
            public Tilemap light;
            public Tilemap shadow;
            public Tilemap rock;
        }
        
        [SerializeField] private TileMapGroup tileMaps;
        public TileMapGroup TileMaps => tileMaps;
    
        private void Awake() => ServiceLocator.Instance.RegisterService(this);
        public bool CanPlayerMove(Vector3 direction)
        {
            var nextCell = grid.WorldToCell(player.position + direction.normalized);
        
            if (tileMaps.border.HasTile(nextCell) || tileMaps.rock.HasTile(nextCell)) return false;

            if (tileMaps.light.HasTile(nextCell) && tileMaps.light.GetTile(nextCell) != shadowTile) return false;

            return tileMaps.floor.HasTile(nextCell);
        }

        public bool CanEnemyMove(Vector3 currentPos, Vector3 direction, int speed = 1)
        {
            var nextCell = grid.WorldToCell(currentPos + (direction) * speed);
            if (tileMaps.border.HasTile(nextCell) || tileMaps.rock.HasTile(nextCell)) return false;
            return tileMaps.floor.HasTile(nextCell);
        }
        public bool IsShadowTile(Vector3Int pos) //TileHasShadow
        {
            return !(tileMaps.light.GetTile(pos) == shadowTile);
        }
        public bool IsInLight() //Should be changed to TileHasLight
        {
            var currentCell = grid.WorldToCell(player.position);
            return (tileMaps.light.HasTile(currentCell) && tileMaps.light.GetTile(currentCell) != shadowTile);
        }

        public bool CanPlaceShadow(Vector3Int pos)
        {
            return tileMaps.floor.HasTile(pos) && !tileMaps.rock.HasTile(pos);
        }
        public Vector3Int PlayerTile()
        {
            return PositionInGrid(player.position);
        }
        public Vector3Int PositionInGrid(Vector3 position)
        {
            return grid.WorldToCell(position);
        }
        
        public IEnumerable<Vector3Int> GetCellsInRadius(Vector3Int center, int radius)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius) // Circular radius
                    {
                        yield return new Vector3Int(center.x + x, center.y + y, 0);
                    }
                }
            }
        }
    }
}
