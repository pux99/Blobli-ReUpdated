using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class GemManager : MonoBehaviour
{
    public GameObject gemPrefab;
    private GemPool _gemPool;

    private void Awake()
    {
        _gemPool = new GemPool(gemPrefab, transform, 10);
        ServiceLocator.Instance.RegisterService(this);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Gem gem = _gemPool.Get(GemType.Shapire);
            gem.GameObject.transform.position = Random.insideUnitSphere * 5f;

            StartCoroutine(ReturnToPoolAfterDelay(gem, 2f));
        }
    }

    private IEnumerator ReturnToPoolAfterDelay(Gem gem, float delay)
    {
        yield return new WaitForSeconds(delay);
        _gemPool.Release(gem);
    }
}
