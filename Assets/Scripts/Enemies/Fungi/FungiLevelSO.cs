using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Enemies/FungiLevel")]
public class FungiLevelSO : ScriptableObject
{
    [Header("Generic for all")]
    [SerializeField] protected internal Sprite offSprite;
    [SerializeField] protected internal Sprite onSprite;

    [Header("Fungi specific")]
    [SerializeField] protected internal List<FungiStats> fungi;
}
