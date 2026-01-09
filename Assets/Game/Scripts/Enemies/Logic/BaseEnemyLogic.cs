using Core.Components;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using Enemies.Signals;
using Zenject;

namespace Enemies.Logic
{
    public abstract class BaseEnemyLogic
    {
        protected CustomPhysics Physics;
        protected PoolableObject PoolableObject;
        protected HealthSystem HealthSystem; 
        protected CollisionHandler CollisionHandler;
        protected SignalBus SignalBus;
        protected ParticleService ParticleService;
        
        protected abstract EnemyType Type { get; }

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
