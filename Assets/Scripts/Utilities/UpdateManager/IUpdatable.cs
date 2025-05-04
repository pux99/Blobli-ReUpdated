namespace Utilities.UpdateManager
{
    public interface IUpdatable
    {
        void Tick(float deltaTime); // Se llama una vez por frame desde el CustomUpdateManager
    }
}