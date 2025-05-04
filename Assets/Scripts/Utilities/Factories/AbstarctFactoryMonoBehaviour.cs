using UnityEngine;
namespace Factory.Essentials
{
    public abstract class AbstractFactoryMonoBehaviour<TObject, TConfig> where TObject : MonoBehaviour ,IConfigurable<TConfig>
    {
        protected abstract TObject Prefab { get; }
        public virtual TObject Create( TConfig config)
        {
            var newBorn = Object.Instantiate(Prefab);
            newBorn.Configure(config);
            return newBorn;
        }
    }
}
