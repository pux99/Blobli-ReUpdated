using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Enemies/Fungi_Config")]
public class FungiSO : ScriptableObject
{
    public TileBase[] tileVariants;
    public Sprite offSprite;
    public Sprite onSprite;

    public TileBase RandomLightTile()
    {
        return tileVariants[Random.Range(0, tileVariants.Length)];
    }
}
