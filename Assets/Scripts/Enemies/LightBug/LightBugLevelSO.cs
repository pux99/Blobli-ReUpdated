using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Enemies/LightBug_Level")]
public class LightBugLevelSO : ScriptableObject
{
    [Header("Generic for all")]
    public Sprite[] animationFrames;
    
    [Header("LightBug specific")]
    public List<LightBugStats> lightBugs;
}
