using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] public Grid grid;
    [Header("Tiles")] 
    [SerializeField] private TileBase shadowTile;
        public TileBase ShadowTile => shadowTile;
        
    [System.Serializable]
    public class TilemapGroup
    {
        public Tilemap border;
        public Tilemap floor;
        public Tilemap light;
        public Tilemap shadow;
        public Tilemap rock;
    }

    [SerializeField] private TilemapGroup tilemaps;
    public TilemapGroup Tilemaps => tilemaps;
    
    private void Awake() => ServiceLocator.Instance.RegisterService(this);
    public bool CanMove(Vector3 direction) //For the player
    {
        var nextCell = grid.WorldToCell(player.position + direction);
        
        if (tilemaps.border.HasTile(nextCell) || tilemaps.rock.HasTile(nextCell)) return false;

        if (tilemaps.light.HasTile(nextCell) && tilemaps.light.GetTile(nextCell) != shadowTile) return false;

        return tilemaps.floor.HasTile(nextCell);
    }
    public bool IsShadowTile(Vector3Int pos)
    {
        return !(tilemaps.light.GetTile(pos) == shadowTile);
    }
    public bool IsInLight() //For the player
    {
        var currentCell = grid.WorldToCell(player.position);
        return (tilemaps.light.HasTile(currentCell) && tilemaps.light.GetTile(currentCell) != shadowTile);
    }
    public bool CanPlaceShadow(Vector3Int pos)
    {
        return tilemaps.floor.HasTile(pos);
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
