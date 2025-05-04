using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.UpdateManager
{
    public class CustomUpdateManager : MonoBehaviour
    {
        private List<IUpdatable> updatables = new List<IUpdatable>();
        
        private void Awake()
        {
            ServiceLocator.Instance.RegisterService(this);
        }

        void Update()
        {
            foreach (var u in updatables)//Preguntar que pasa si un elemento es removido durante el for each y si eso lo rompe ,ver como evitarlo
            {
                u.Tick(Time.deltaTime);
            }
        }
        
        public void Register(IUpdatable updatable)
        {
            if (!updatables.Contains(updatable))
                updatables.Add(updatable);
        }
        
        public void Unregister(IUpdatable updatable)
        {
            updatables.Remove(updatable);
        }
    }
}