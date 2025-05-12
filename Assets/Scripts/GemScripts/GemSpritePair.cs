using System;
using System.Collections.Generic;
using GemScripts;
using UnityEngine;

[CreateAssetMenu(fileName = "Gem-Sprite-Pair", menuName = "ScriptableObjects/Gem-Sprite-Pair", order = 1)]
public class GemSpritePair : ScriptableObject
{
    [SerializeField] private List<GemSpritePairForArray> PairArray;
    public Dictionary<Sprite,GemType> keyValuePairs = new Dictionary<Sprite,GemType>();
    private void OnValidate()
    {
        keyValuePairs.Clear();
        if (PairArray != null)
        {
            foreach (var Pair in PairArray)
            {
                keyValuePairs.Add(Pair.sprite, Pair.gemType);
            }
        }
    }
}
[Serializable]
public class GemSpritePairForArray
{
    public Sprite sprite;
    public GemType gemType;
}
