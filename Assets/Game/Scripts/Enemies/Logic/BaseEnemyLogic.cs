using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Configuration;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using Enemies.Signals;
using UnityEngine;
using Zenject;

namespace Enemies.Logic
{
    public abstract class BaseEnemyLogic
    {
        protected abstract EnemyType Type { get; }
        
        protected EnemySettings _settings;
        protected CustomPhysics _physics;
        protected PoolableObject _poolableObject;
        protected HealthSystem _healthSystem; 
        protected CollisionHandler _collisionHandler;
        protected SignalBus _signalBus;

        public abstract void Move();

        public virtual void OnPresentationEnabled()
        {
            _healthSystem.RestoreHealth();
        }
        
        protected virtual void GetDestroyed()
        {
            _signalBus.TryFire(new EnemyDestroyedSignal(Type));
            
            _poolableObject.Despawn();
        }
    }
}
