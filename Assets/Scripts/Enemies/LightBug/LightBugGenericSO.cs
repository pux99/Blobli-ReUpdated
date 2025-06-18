using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Enemies/LightBug_Config")]
public class LightBugGenericSO : ScriptableObject
{
    [Header("Animation")]
    public Sprite[] animationFrames;

    [Header("Light Tile Variants")]
    public TileBase[] tileVariants;
}
