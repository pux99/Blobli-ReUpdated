using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Enemies/LightBug_Level")]
public class LightBugLevelSO : ScriptableObject
{
    [SerializeField] protected internal List<Vector3Int>[] bugPaths;
    [SerializeField] protected internal int[] bugIntensities;
    [SerializeField] protected internal int[] bugSpeeds;
}
