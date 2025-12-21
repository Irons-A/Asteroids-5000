using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Systems.ObjectPools;
using Enemies.Logic;
using UnityEngine;
using Zenject;

namespace Enemies.Presentation
{
    public class SmallAsteroidPresentation : EnemyPresentation
    {
        private SmallAsteroidLogic _logic;
        
        [Inject]
        private void Construct(SmallAsteroidLogic logic)
        {
            _logic = logic;
        }

        protected override void Awake()
        {
            _shouldTeleport = true;
            
            PoolableObject = GetComponent<PoolableObject>();
            CollisionHandler = GetComponent<CollisionHandler>();
            
            _logic.Configure(this, PoolableObject, CollisionHandler);
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
