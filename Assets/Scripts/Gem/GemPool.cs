using UnityEngine;
using UnityEngine.Pool;

public class GemPool
{
    private readonly ObjectPool<Gem> _pool;

    public GemPool(GameObject prefab, Transform parent = null, int defaultCapacity = 10, int maxSize = 100)
    {
        _pool = new ObjectPool<Gem>(
            createFunc: () =>
            {
                GameObject go = Object.Instantiate(prefab, parent);
                go.SetActive(false);
                return new Gem(go);
            },
            actionOnGet: gem => gem.Activate(),
            actionOnRelease: gem => gem.Deactivate(),
            actionOnDestroy: gem => Object.Destroy(gem.GameObject),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public Gem Get(GemType type)
    {
        Gem gem = _pool.Get();
        gem.Type = type;
        return gem;
    }

    public void Release(Gem gem) => _pool.Release(gem);
}