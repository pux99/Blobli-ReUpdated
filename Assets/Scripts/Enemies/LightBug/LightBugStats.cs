using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class LightBugStats
{
    public GameObject lightBugGameObject;
    public Vector3Int initialPosition;
    public List<Vector2Int> path;
    public int lightIntensity;
    public int speed;
}
