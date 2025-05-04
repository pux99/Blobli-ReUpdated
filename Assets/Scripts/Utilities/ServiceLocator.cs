using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoBehaviour
{
    [SerializeField]private static ServiceLocator _instance;
    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("service Locator", typeof(ServiceLocator)).GetComponent<ServiceLocator>();
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake() 
    { 
        if (_instance == null )
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        } 
        else 
        { 
            Destroy(this.gameObject);
        } 
    }
    
    private readonly Dictionary<Type, object> _services=new Dictionary<Type, object>();

    public void RegisterService<T>(T service)
    {
        if (!_services.ContainsKey(typeof(T)))
        {
            _services.Add(typeof(T),service);
        }
        else
        {
            _services[typeof(T)] = service;
        }
    }

    public T GetService<T>()
    {
        if (_services.TryGetValue(typeof(T), out object service))
        {
            return (T)service;
        }
        else
        {
            Debug.Log($"there is no register of the type {typeof(T)}");
            return default;
        }
    }
    
}