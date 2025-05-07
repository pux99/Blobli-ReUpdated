namespace Utilities.Factories
{
  public abstract class AbstractFactory<TObject, TConfig> 
        where TObject : IConfigurable<TConfig>, new()
    {
        public virtual TObject Create( TConfig config)
        {
            var newborn = new TObject();
            newborn.Configure(config);
            return newborn;
        }
    }
}
