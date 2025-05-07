using System.Collections.Generic;
using UnityEngine;

namespace Potions
{
    [CreateAssetMenu(menuName = "ShadowShape")]
    public class SO_ShadowShape : ScriptableObject
    {
        public List<Vector2Int> relativePositions;
    }
}
