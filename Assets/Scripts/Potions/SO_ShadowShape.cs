using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Potions
{
    [CreateAssetMenu(menuName = "ShadowShape")]
    public class SO_ShadowShape : ScriptableObject
    {
        public Sprite sprite;
        public List<Vector2Int> relativePositions;
    }
}
