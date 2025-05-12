using System;
using GemScripts;
using UnityEngine;

namespace Potions
{
    [Serializable]
    public class PotionRecipe
    {
        [SerializeField]private GemType gem1;
        [SerializeField]private GemType gem2;
        [SerializeField]private SO_ShadowShape potion;

        public SO_ShadowShape Craft(GemType one, GemType two)
        {
            if ((one == gem1 && two == gem2) || (one == gem2 && two == gem1))
                return potion;
            return default;
        }
    }
}