namespace Utilities.Factories
{
        public interface IConfigurable<in TConfig>
        {
                void Configure(TConfig config);
        }
}
