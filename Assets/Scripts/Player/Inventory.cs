using System;
using System.Linq;
using GemScripts;
using Potions;
using UnityEngine;

namespace Player
{
    public class Inventory
    {
        
        private Gem[] _stored = new Gem[4];
        private Gem[] _keys = new Gem[2];
        private Gem[] _crating = new Gem[2];
        private SO_ShadowShape _potion;
        
        public Action<Gem[]> UpdateStored;
        public Action<Gem[]> UpdateKeys;
        public Action<Gem[]> UpdateCrafting;
        public Action UpdateCraftingNoList;
        public Action<SO_ShadowShape> UpdatePotion;
        public Action<SO_ShadowShape> usePotion;

        private PotionRecipes recipes;
        public Inventory(PotionRecipes recipes)
        {
            ServiceLocator.Instance.GetService<GemManager>().playerInventory = this;
            UpdateCrafting += CheckForRecipe;
            this.recipes = recipes;
        }
        public bool TryAddGem(Gem gem)
        {
            if (gem.Type == GemType.KeyGem)
            {
                for (int i = 0; i < _keys.Length; i++)
                {
                    if (_keys[i] != null) continue;
                    _keys[i] = gem;
                    UpdateKeys?.Invoke(_keys);
                    return true;
                }
                return false;
            }
            for (int i = 0; i < _stored.Length; i++)
            {
                if (_stored[i] != null) continue;
                _stored[i] = gem;
                UpdateStored?.Invoke(_stored);
                return true;
            }
            return false;
        }
        public void ToCrafting(int slot)
        {
            if (slot is < 0 or >= 4||_stored[slot]==_crating[0]||_stored[slot]==_crating[1]) return;
            for (int i = 0; i < _crating.Length; i++)
            {
                if (_crating[i] != null || _stored[slot] == null) continue;
                _crating[i] = _stored[slot];
                UpdateCrafting?.Invoke(_crating);
                break;
            }
        }
        public void RemoveFromCraft(int slot)
        {
            _crating[slot] = null;
            UpdateCrafting?.Invoke(_crating);
            UpdateCraftingNoList?.Invoke();
        }

        public void RemoveGem(int slot)
        {
            _stored[slot]=null;
            UpdateStored?.Invoke(_stored);
        }

        public void ThrowPotion()
        {
            if(_potion!=default)
                usePotion?.Invoke(_potion);
        }

        private void CheckForRecipe(Gem[] gems)
        { 
            if (gems[0] == null || gems[1] == null)
            {
                UpdatePotion?.Invoke(default);
                _potion = default;
                return;
            }
            var newPotion = recipes.CheckForRecipe(gems[0].Type, gems[1].Type);
            if (newPotion == default) return;
            _potion = newPotion;
            UpdatePotion?.Invoke(_potion);
        }

        public void ClearCrating()
        {
            for (int j = 0; j < _crating.Length; j++)
            {
                for (int i = 0; i < _stored.Length; i++)
                    if (_crating[j] == _stored[i])
                        RemoveGem(i);
                RemoveFromCraft(j);
            }
        }

        public bool CheckKeys()
        {
            return _keys.All(key => key != null);
        }
    }
}
