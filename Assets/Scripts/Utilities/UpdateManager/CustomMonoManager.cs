using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.MonoManager
{
    public class CustomMonoManager : MonoBehaviour
    {
        private List<IUpdatable> _updatables = new List<IUpdatable>();
        private List<IStartable> _startables = new List<IStartable>();

        private void Awake()
        {
            ServiceLocator.Instance.RegisterService(this);
        }
        void Start()
        {
            foreach (var u in _startables)//Preguntar que pasa si un elemento es removido durante el for each y si eso lo rompe ,ver como evitarlo
            {
                u.Beginning();
            }
        }
        void Update()
        {
            foreach (var u in _updatables)//Preguntar que pasa si un elemento es removido durante el for each y si eso lo rompe ,ver como evitarlo
            {
                u.Tick(Time.deltaTime);
            }
        }
        
        public void RegisterOnUpdate(IUpdatable updatable)
        {
            if (!_updatables.Contains(updatable))
                _updatables.Add(updatable);
        }
        
        public void UnregisterOnUpdate(IUpdatable updatable)
        {
            _updatables.Remove(updatable);
        }

        public void RegisterOnStart(IStartable startable)
        {
            if (!_startables.Contains(startable))
                _startables.Add(startable);
        }

        public void UnregisterOnStart(IStartable startable)
        {
            _startables.Remove(startable);
        }



    }
}