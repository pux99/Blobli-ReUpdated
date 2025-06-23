using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class LightBugStats
{
    public GameObject lightBugGameObject;
    public Vector3Int initialPosition;
    public List<Vector3Int> directions;
    public int lightIntensity;
    public int speed;
}
