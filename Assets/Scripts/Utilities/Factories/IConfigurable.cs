namespace Factory.Essentials
{
        public interface IConfigurable<in TConfig>
        {
                void Configure(TConfig config);
        }
}
