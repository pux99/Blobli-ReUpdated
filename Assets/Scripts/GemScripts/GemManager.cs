using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace GemScripts
{
    public class GemManager : MonoBehaviour
    {
        [SerializeField] private GemSpritePair gemSpritePair;
        private GemPool _gemPool;
        private Dictionary<GameObject, Gem> _gems = new Dictionary<GameObject, Gem>();
        public GameObject gemPrefab;
        
        public Inventory PlayerInventory;
        private void Awake()
        {
            _gemPool = new GemPool(gemSpritePair,gemPrefab, transform, 10);
            ServiceLocator.Instance.RegisterService(this);
            SetUpGems();

        }

        public void SetInventory(Inventory inventory)
        {
            PlayerInventory = inventory;
        }
        
        private void SetUpGems()
        {
            foreach (var item in GetComponentsInChildren<SpriteRenderer>())
            {
                Gem gem = _gemPool.CreateGemWithGameobject(gemSpritePair.keyValuePairs[item.sprite], item.gameObject);
                _gems.Add(gem.GameObject, gem);
                gem.GameObject.transform.position = item.transform.position;
            }
        }
        public Gem PickupGem(Vector3 pos)
        {
            foreach (var obj in _gems.Keys)// i think that this is more performant than an overlap circle in the position of the player
            {
                float dist = Vector2.Distance(pos, obj.transform.position);
                if (dist < .4f)
                {
                    var g = _gems[obj];
                    //RemoveGem(obj);
                    return g;
                }
            }
            return default;
        }

        public void RemoveGem(GameObject go)
        {
            _gems.Remove(go);
        }
        
        public Gem GetGem(GameObject GO)
        {
            _gems.TryGetValue(GO, out var gem);
            return gem;
        }

    }
}
