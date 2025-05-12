namespace Utilities.MonoManager
{
    public interface IStartable
    {
        void Beginning(); // Se llama una vez al inicio del runtime desde el CustomMonoManager
    }
}
