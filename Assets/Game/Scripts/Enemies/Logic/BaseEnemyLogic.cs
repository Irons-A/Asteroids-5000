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
        
        protected EnemySettings Settings;
        protected CustomPhysics Physics;
        protected PoolableObject PoolableObject;
        protected HealthSystem HealthSystem; 
        protected CollisionHandler CollisionHandler;
        protected SignalBus SignalBus;

        public abstract void Move();

        public virtual void OnPresentationEnabled()
        {
            HealthSystem.RestoreHealth();
        }
        
        protected virtual void GetDestroyed()
        {
            SignalBus.TryFire(new EnemyDestroyedSignal(Type));
            
            PoolableObject.Despawn();
        }
    }
}
