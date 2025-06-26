using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/GenericInformation")]
public class EnemySO : ScriptableObject
{
    [Header("Fungi animation")]
    [SerializeField] protected internal Sprite offSprite;
    [SerializeField] protected internal Sprite onSprite;
    
    [Header("LightBug animation")]
    [SerializeField] protected internal Sprite[] animationFrames;
}
