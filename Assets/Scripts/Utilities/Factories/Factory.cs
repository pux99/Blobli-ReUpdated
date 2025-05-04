namespace Factory.Essentials
{
    public class Factory<TObject, TConfig> : AbstractFactory<TObject, TConfig>
        where TObject : IConfigurable<TConfig>, new()
    { 
        public override TObject Create(TConfig config)
        {
           return base.Create(config);
        }
    }
}