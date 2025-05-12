using System.Collections.Generic;
using System.Linq;
using GemScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Potions
{
    [CreateAssetMenu(fileName = "PotionRecipes", menuName = "ScriptableObjects/PotionRecipes")]
    public class PotionRecipes : ScriptableObject
    {
        [SerializeField]private List<PotionRecipe> recipes;

        public SO_ShadowShape CheckForRecipe(GemType gem1, GemType gem2)
        {
            return recipes.Select(recipe => recipe.Craft(gem1, gem2)).FirstOrDefault(newPotion => newPotion != default);
        }
    }
}
