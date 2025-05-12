using System;
using Player;
using UnityEngine;

namespace Utilities
{
    public class GameManager : MonoBehaviour
    {
        public static event Action OnStepTaken;
        private void Awake() => ServiceLocator.Instance.RegisterService(this);

        [SerializeField] private int stepCounter;
        
        public void Step()
        {
            stepCounter++;
            OnStepTaken?.Invoke();
        }
    }
}
