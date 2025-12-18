using System;
using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Systems.ObjectPools;
using Enemies.Logic;
using UnityEngine;
using Zenject;

namespace Enemies.Presentation
{
    public class BigAsteroidPresentation : EnemyPresentation
    {
        private BigAsteroidLogic _logic;
        private PoolableObject _poolableObject;
        private CollisionHandler _collisionHandler;
        
        [Inject]
        private void Construct(BigAsteroidLogic logic)
        {
            _logic = logic;
        }

        protected override void Awake()
        {
            base.Awake();
            
            _poolableObject = GetComponent<PoolableObject>();
            _collisionHandler = GetComponent<CollisionHandler>();
            
            _logic.Configure(this, _poolableObject, _collisionHandler);
        }

        private void FixedUpdate()
        {
            _logic.Move();
        }

        private void OnEnable()
        {
            _logic.OnPresentationEnabled();
        }
    }
}
