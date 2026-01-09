using System;
using Core.Components;
using Core.Configuration;
using Core.Configuration.Enemies;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using Enemies.Presentation;
using Zenject;

namespace Enemies.Logic
{
    public class BigAsteroidLogic : BaseEnemyLogic, IDisposable
    {
        private readonly BigAsteroidSettings _settings;
        private readonly SmallAsteroidSpawner _smallAsteroidSpawner;
        
        private BigAsteroidPresentation _presentation;
        
        protected override EnemyType Type => EnemyType.BigAsteroid;
        
        public BigAsteroidLogic(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            ParticleService particleService, SignalBus signalBus, SmallAsteroidSpawner smallAsteroidSpawner)
        {
            _settings = configProvider.BigAsteroidSettingsRef;
            Physics = physics;
            HealthSystem = healthSystem;
            ParticleService = particleService;
            SignalBus = signalBus;
            _smallAsteroidSpawner = smallAsteroidSpawner;
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
            _smallAsteroidSpawner.SpawnSmallAsteroids(_settings.MinSmallAsteroidSpawnAmount,
                _settings.MaxSmallAsteroidSpawnAmount, _presentation.transform.position);
            
            ParticleService.SpawnParticles(PoolableObjectType.ExplosionParticles, _presentation.transform.position);
            
            base.GetDestroyed();
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
