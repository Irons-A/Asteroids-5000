using Core.Systems.ObjectPools;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Core.Logic;
using UnityEngine;
using Zenject;

namespace Core.Components
{
    [RequireComponent(typeof(CollisionHandler))]
    public class Projectile : MonoBehaviour
    {
        private PoolableObject _poolableObject;
        private ProjectileLogic _logic;
        private CollisionHandler _collisionHandler;

        [Inject]
        private void Construct(ProjectileLogic logic)
        {
            _logic = logic;
            _logic.SetPresentationTransform(transform);
            _logic.OnDelayedDestructionCalled += CallDespawn;
        }
        
        private void Awake()
        {
            _collisionHandler = GetComponent<CollisionHandler>();
            _collisionHandler.OnDestructionCalled += CallDespawn;
            
            if (TryGetComponent(out PoolableObject poolableObject))
            {
                _poolableObject = poolableObject;
            }
        }

        private void OnDestroy()
        {
            _logic.OnDelayedDestructionCalled -= CallDespawn;
            _collisionHandler.OnDestructionCalled -= CallDespawn;
            _logic.CancelDelayedDestruction();
        }

        private void Update()
        {
            _logic.MoveProjectile();
        }

        public void Configure(float speed, bool delayedDestruction = false, float destroyAfter = 1)
        {
            _logic.ConfigureParameters(speed, delayedDestruction, destroyAfter);
        }

        private void CallDespawn()
        {
            _logic.CancelDelayedDestruction();

            if (_poolableObject != null)
            {
                _poolableObject.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
