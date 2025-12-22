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
        
        [Inject]
        private void Construct(BigAsteroidLogic logic)
        {
            _logic = logic;
        }

        protected override void Awake()
        {
            base.Awake();
            
            PoolableObject = GetComponent<PoolableObject>();
            CollisionHandler = GetComponent<CollisionHandler>();
            
            _logic.Configure(this, PoolableObject, CollisionHandler);
        }
        
        private void OnEnable()
        {
            _logic.OnPresentationEnabled();
            SetAngle(0, false, true);
        }

        private void FixedUpdate()
        {
            _logic.Move();
        }
    }
}
