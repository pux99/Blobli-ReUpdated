using Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy
{
    protected GridManager GridManager;
    protected Vector3Int CellPos;
    protected Tilemap LightMap;
    protected SpriteRenderer SpriteRenderer;

    public virtual void OnSceneChange()
    {
        
    }
}
