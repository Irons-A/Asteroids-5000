using System;
using System.Collections;
using System.Collections.Generic;
using Core.Systems.ObjectPools;
using Enemies.Logic;
using UnityEngine;
using Zenject;

namespace Enemies.Presentation
{
    [RequireComponent((typeof(PoolableObject)))]
    public class BigAsteroidPresentation : EnemyPresentation
    {
        private BigAsteroidLogic _logic;
        private PoolableObject _poolableObject;
        
        [Inject]
        private void Construct(BigAsteroidLogic logic)
        {
            _logic = logic;
        }

        protected override void Awake()
        {
            base.Awake();
            _poolableObject = GetComponent<PoolableObject>();
            _logic.Configure(this, _poolableObject);
        }

        private void FixedUpdate()
        {
            _logic.ProcessFixedUpdate();
        }

        private void OnEnable()
        {
            _logic.OnPresentationEnabled();
        }

        private void OnDisable()
        {
            
        }
    }
}
