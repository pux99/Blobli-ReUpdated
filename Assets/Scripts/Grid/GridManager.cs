using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] public Grid grid;

    private Vector3Int nextCell;
    
    [Header("TileMaps")]
    [SerializeField] private Tilemap floorTileMap;
    [SerializeField] private Tilemap borderTileMap;
    [SerializeField] private Tilemap rockTileMap;
    [SerializeField] private Tilemap lightTileMap;
    
    public bool CanMove(Vector3 direction)
    {
        nextCell = grid.WorldToCell(player.position + direction);
        
        if (borderTileMap.HasTile(nextCell) || rockTileMap.HasTile(nextCell) || lightTileMap.HasTile(nextCell))
            return false;
        
        return floorTileMap.HasTile(nextCell);
    }
}
