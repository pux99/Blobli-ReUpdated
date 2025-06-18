using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Enemies/FungiLevel")]
public class FungiLevelSO : ScriptableObject
{
    [Header("Geneic for all")]
    [SerializeField] protected internal TileBase[] tileVariants;
    [SerializeField] protected internal Sprite offSprite;
    [SerializeField] protected internal Sprite onSprite;
    
    [Header("Fungi specific")]
    [SerializeField] protected internal int[] onToOffList;
    [SerializeField] protected internal int[] offToOnList;
    
    public TileBase RandomLightTile()
    {
        return tileVariants[Random.Range(0, tileVariants.Length)];
    }
}
