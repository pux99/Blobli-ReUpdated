using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
public class GemManager : MonoBehaviour
{
    public GameObject gemPrefab;
    private GemPool _gemPool;
    [SerializeField] GemSpritePair _gemSpritePair;
    public Dictionary<GameObject, Gem> Gems = new Dictionary<GameObject, Gem>();
    public List<SpriteRenderer> Renderers = new List<SpriteRenderer>();
    private void Awake()
    {
        _gemPool = new GemPool(_gemSpritePair,gemPrefab, transform, 10);
        ServiceLocator.Instance.RegisterService(this);
        SetUpGems();

    }
    private void SetUpGems()
    {
        foreach (var item in GetComponentsInChildren<SpriteRenderer>())
        {
            Renderers.Add(item);
        }
        foreach (var item in Renderers)
        {
            Gem gem = _gemPool.CreateGremWithGameobject(_gemSpritePair.keyValuePairs[item.sprite], item.gameObject);
            Gems.Add(gem.GameObject, gem);
            gem.GameObject.transform.position = item.transform.position;
            Debug.Log(gem.Type);
        }
    }
    public Gem PickupGem(Vector3 pos)
    {
        foreach (var obj in Gems.Keys)// i think that this is more performant than an overlap circle in the position of the player
        {
            float dist = Vector2.Distance(pos, obj.transform.position);
            if (dist < .4f)
            {
                return Gems[obj];
            }
        }
        return default;
    }
    void GetNewGem()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Gem gem = _gemPool.Get(GemType.Shapire);
            Gems.Add(gem.GameObject,gem);
        }
    }

    public Gem GetGem(GameObject GO)
    {
        Gems.TryGetValue(GO, out var gem);
        return gem;
    }

}
