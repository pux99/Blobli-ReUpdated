using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class GemManager : MonoBehaviour
{
    public GameObject gemPrefab;
    private GemPool _gemPool;
    public Dictionary<GameObject, Gem> Gems = new Dictionary<GameObject, Gem>();

    private void Awake()
    {
        _gemPool = new GemPool(gemPrefab, transform, 10);
        ServiceLocator.Instance.RegisterService(this);
    }
    void GetNewGem()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Gem gem = _gemPool.Get(GemType.Shapire);
            gem.GameObject.transform.position = Random.insideUnitSphere * 5f;
            Gems.Add(gem.GameObject,gem);
        }
    }

    public Gem GetGem(GameObject GO)
    {
        Gems.TryGetValue(GO, out var gem);
        return gem;
    }

}
