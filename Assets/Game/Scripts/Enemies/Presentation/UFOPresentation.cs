using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Systems.ObjectPools;
using Enemies.Logic;
using UnityEngine;
using Zenject;

namespace Enemies.Presentation
{
    public class UFOPresentation : EnemyPresentation
    {
        private UFOLogic _logic;
        
        [field: SerializeField] public Transform[] Firepoints { get; private set; }
        
        [Inject]
        private void Construct(UFOLogic logic)
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
        }

        private void Update()
        {
            _logic.RotateTowardsPlayer();
        }

        private void FixedUpdate()
        {
            _logic.Move();
        }
    }
}
