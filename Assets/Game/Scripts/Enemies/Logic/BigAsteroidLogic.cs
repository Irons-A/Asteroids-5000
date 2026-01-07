using System;
using Core.Components;
using Core.Configuration;
using Core.Configuration.Enemies;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using Enemies.Presentation;
using Enemies.Signals;
using Zenject;
using Random = UnityEngine.Random;

namespace Enemies.Logic
{
    public class BigAsteroidLogic : BaseEnemyLogic, IDisposable
    {
        private readonly PoolAccessProvider _objectPool;
        private readonly BigAsteroidSettings _settings;
        
        private BigAsteroidPresentation _presentation;

        private int _minSmallAsteroidSpawnAmount;
        private int _maxSmallAsteroidSpawnAmount;
        
        protected override EnemyType Type => EnemyType.BigAsteroid;
        
        public BigAsteroidLogic(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            PoolAccessProvider accessProvider, SignalBus signalBus, ParticleService  particleService)
        {
            _settings = configProvider.BigAsteroidSettingsRef;
            Physics = physics;
            HealthSystem = healthSystem;
            _objectPool = accessProvider;
            SignalBus = signalBus;
            ParticleService = particleService;
        }
        
        public void Configure(BigAsteroidPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            
            Physics.SetMovableObject(_presentation, _settings.Mass);
            
            PoolableObject = presentationPoolableObject;
            
            CollisionHandler = collisionHandler;
            CollisionHandler.Configure(_settings.Damage, EntityAffiliation.Enemy, 
                EntityDurability.Piercing, shouldCauseRicochet: true, customPhysics: Physics);
            CollisionHandler.OnDamageReceived += HealthSystem.TakeDamage;
            CollisionHandler.OnDestructionCalled += GetDestroyed;
            CollisionHandler.OnRicochetCalled += Physics.ApplyRicochet;
            
            HealthSystem.Configure(_settings.Health, true);
            HealthSystem.OnHealthDepleted += GetDestroyed;
        }

        public override void Move()
        {
            Physics.SetInstantVelocity(_settings.Speed);
            Physics.ProcessPhysics();
        }

        protected override void GetDestroyed()
        {
            SpawnSmallAsteroids();
            
            ParticleService.SpawnParticles(PoolableObjectType.ExplosionParticles, _presentation.transform.position);
            
            base.GetDestroyed();
        }

        private void SpawnSmallAsteroids()
        {
            int asteroidsToSpawn = Random.Range(_settings.MinSmallAsteroidSpawnAmount,
                _settings.MaxSmallAsteroidSpawnAmount + 1);
            
            for (int i = 0; i < asteroidsToSpawn; i++)
            {
                PoolableObject smallAsteroid = _objectPool.GetFromPool(PoolableObjectType.SmallAsteroid);
                
                smallAsteroid.transform.position = _presentation.transform.position;
                
                SignalBus.TryFire(new EnemySpawnedSignal());
            }
        }
        
        public void Dispose()
        {
            if (HealthSystem != null) HealthSystem.OnHealthDepleted -= GetDestroyed;
            
            if (CollisionHandler != null)
            {
                CollisionHandler.OnDamageReceived -= HealthSystem.TakeDamage;
                CollisionHandler.OnDestructionCalled -= GetDestroyed;
                CollisionHandler.OnRicochetCalled -= Physics.ApplyRicochet;
            }
        }
    }
}
