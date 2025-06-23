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
        
        // ────────────────────── Position Utilities ──────────────────────
        public Vector3Int WorldToCell(Vector3 position) => grid.WorldToCell(position);
        public Vector3Int PlayerTile() => WorldToCell(player.position);

        // ────────────────────── Movement Checks ──────────────────────
        public bool CanPlayerMove(Vector3 direction)
        {
            Vector3Int nextCell = WorldToCell(player.position + direction.normalized);

            if (HasAnyTile(nextCell, tileMaps.border, tileMaps.rock)) return false;
            if (tileMaps.light.HasTile(nextCell) && !IsShadowTile(nextCell)) return false;

            return tileMaps.floor.HasTile(nextCell);
        }
        
        // ────────────────────── Tile Checks ──────────────────────
        public bool CanPlaceTile(Vector3Int cell)
        {
            if (HasAnyTile(cell, tileMaps.light, tileMaps.rock, tileMaps.shadow)) return false;
            return tileMaps.floor.HasTile(cell);
        }

        public bool CanPlaceShadow(Vector3Int cell)
        {
            return tileMaps.floor.HasTile(cell) && !tileMaps.rock.HasTile(cell);
        }

        public bool IsShadowTile(Vector3Int pos)
        {
            return tileMaps.light.GetTile(pos) == shadowTile;
        }

        public bool TileHasLight(Vector3Int pos)
        {
            return tileMaps.light.HasTile(pos) && !IsShadowTile(pos);
        }
        
        // ────────────────────── Radius Query ──────────────────────
        public List<Vector3Int> GetCellsInRadius(Vector3Int center, int radius)
        {
            List<Vector3Int> result = new();
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x * x + y * y <= radius * radius)
                        result.Add(new Vector3Int(center.x + x, center.y + y, 0));
                }
            }
            return result;
        }

        // ────────────────────── Helpers ──────────────────────
        private bool HasAnyTile(Vector3Int cell, params Tilemap[] maps)
        {
            foreach (var map in maps)
                if (map.HasTile(cell)) return true;
            return false;
        }
        
    }
}
