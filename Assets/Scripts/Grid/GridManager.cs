using System;
using System.Collections.Generic;
using UnityEditor;
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

        public bool CanEnemyMove(Vector3Int destination)
        {
            if (tileMaps.border.HasTile(destination) || tileMaps.rock.HasTile(destination)) return false;
            return tileMaps.floor.HasTile(destination);
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

        public bool CanPlaceShadow(Vector3Int cell)
        {
            return tileMaps.floor.HasTile(cell) && !tileMaps.rock.HasTile(cell);
        }
        public Vector3Int PlayerTile()
        {
            return WorldToCell(player.position);
        }
        public Vector3Int WorldToCell(Vector3 position)
        {
            return grid.WorldToCell(position);
        }

        public bool CanPlaceTile(Vector3Int cell)
        {
            if (tileMaps.light.HasTile(cell)||tileMaps.rock.HasTile(cell) || tileMaps.shadow.HasTile(cell)) return false;
            return tileMaps.floor.HasTile(cell);
        }
        
        public List<Vector3Int> GetCellsInRadius(Vector3Int center, int radius)
        {
            List<Vector3Int> cellsInRadius = new();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        Vector3Int pos = new(center.x + x, center.y + y, 0);
                        cellsInRadius.Add(pos);
                    }
                }
            }

            return cellsInRadius;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(Vector3.zero, 0.2f);
        }
    }
}
