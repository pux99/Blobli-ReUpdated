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

        [FormerlySerializedAs("tilemaps")] [SerializeField] private TileMapGroup tileMaps;
        public TileMapGroup TileMaps => tileMaps;
    
        private void Awake() => ServiceLocator.Instance.RegisterService(this);
        public bool CanMove(Vector3 direction) //For the player
        {
            var nextCell = grid.WorldToCell(player.position + direction);
        
            if (tileMaps.border.HasTile(nextCell) || tileMaps.rock.HasTile(nextCell)) return false;

            if (tileMaps.light.HasTile(nextCell) && tileMaps.light.GetTile(nextCell) != shadowTile) return false;

            return tileMaps.floor.HasTile(nextCell);
        }
        public bool IsShadowTile(Vector3Int pos)
        {
            return !(tileMaps.light.GetTile(pos) == shadowTile);
        }
        public bool IsInLight() //For the player
        {
            var currentCell = grid.WorldToCell(player.position);
            return (tileMaps.light.HasTile(currentCell) && tileMaps.light.GetTile(currentCell) != shadowTile);
        }
        public bool CanPlaceShadow(Vector3Int pos)
        {
            return tileMaps.floor.HasTile(pos);
        }
        public Vector3Int PlayerTile()
        {
            return grid.WorldToCell(player.position);
        }
        public Vector3Int PositionInGrid(Vector3 position)
        {
            return grid.WorldToCell(position);
        }
    }
}
