using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        [SerializeField] private TileBase[] lightTiles;
        
        public TileBase ShadowTile => shadowTile;
        public TileBase[] LightTileVariants => lightTiles;
        
        [SerializeField] private TileMapGroup tileMaps;
        public TileMapGroup TileMaps => tileMaps;

        private void Awake(){
            ServiceLocator.Instance.RegisterService(this);
            //AsyncOperationHandle<IList<TileBase>> async = Addressables.LoadAssetsAsync<TileBase>("Assets/Art/Tiles/FloorLight/ShadowIndicator.png");
            //async.Completed += AsyncComplet;
        }

        private void Update()
        {
            if (shadowTile == null)
            {
                AsyncOperationHandle<IList<TileBase>> async = Addressables.LoadAssetsAsync<TileBase>("Assets/Art/Tiles/FloorLight/Tile Palette/ShadowIndicator.asset");
                async.Completed += AsyncComplet;
            }
        }

        private void AsyncComplet(AsyncOperationHandle<IList<TileBase>> handle)
        {
            if (handle.Status==AsyncOperationStatus.Succeeded)
            {
                shadowTile = handle.Result[0];
            }
            else
            {
                Debug.Log("puto");
            }
                
        }
        

        // ────────────────────── Position Utilities ──────────────────────
        public Vector3Int WorldToCell(Vector3 position) => grid.WorldToCell(position);
        public Vector3Int PlayerTile() => WorldToCell(player.position);

        // ────────────────────── Movement Checks ──────────────────────
        public bool CanPlayerMove(Vector3 direction)
        {
            Vector3Int nextCell = WorldToCell(player.position + direction.normalized);

            if (HasAnyTile(nextCell, tileMaps.Border, tileMaps.Rock)) return false;
            if (tileMaps.Light.HasTile(nextCell) && !IsShadowTile(nextCell)) return false;

            return tileMaps.Floor.HasTile(nextCell);
        }
        
        // ────────────────────── Tile Checks ──────────────────────
        public bool CanPlaceTile(Vector3Int cell)
        {
            if (HasAnyTile(cell, tileMaps.Light, tileMaps.Rock, tileMaps.Shadow)) return false;
            return tileMaps.Floor.HasTile(cell);
        }

        public bool CanPlaceShadow(Vector3Int cell)
        {
            return tileMaps.Floor.HasTile(cell) && !tileMaps.Rock.HasTile(cell);
        }

        public bool IsShadowTile(Vector3Int pos)
        {
            return tileMaps.Light.GetTile(pos) == shadowTile;
        }

        public bool TileHasLight(Vector3Int pos)
        {
            return tileMaps.Light.HasTile(pos) && !IsShadowTile(pos);
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
