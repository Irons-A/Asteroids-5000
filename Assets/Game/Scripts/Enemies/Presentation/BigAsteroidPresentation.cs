using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Presentation
{
    public class BigAsteroidPresentation : EnemyPresentation
    {
        public event Action OnEnabled;
        public event Action OnDisabled;
        
        private void OnEnable()
        {
            OnEnabled?.Invoke();
        }

        private void OnDisable()
        {
            
        }
    }
}
