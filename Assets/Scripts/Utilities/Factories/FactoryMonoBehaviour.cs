using UnityEngine;

namespace Factory.Essentials
{
    public class FactoryMonoBehaviour<TObject, TConfig> : AbstractFactoryMonoBehaviour<TObject, TConfig>
        where TObject : MonoBehaviour, IConfigurable<TConfig>
    {
        public FactoryMonoBehaviour(TObject prefab)
        {
            this.Prefab = prefab;
        }

        protected override TObject Prefab { get; }
    }
}
