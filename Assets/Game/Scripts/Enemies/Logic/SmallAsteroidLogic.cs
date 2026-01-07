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
    public class SmallAsteroidLogic : BaseEnemyLogic, IDisposable
    {
        private readonly SmallAsteroidSettings _settings;
        
        private SmallAsteroidPresentation _presentation;
        
        protected override EnemyType Type => EnemyType.SmallAsteroid;
        
        public SmallAsteroidLogic(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            SignalBus signalBus, ParticleService  particleService)
        {
            _settings = configProvider.SmallAsteroidSettingsRef;
            Physics = physics;
            HealthSystem = healthSystem;
            SignalBus = signalBus;
            ParticleService = particleService;
        }
        
        public void Configure(SmallAsteroidPresentation presentation, PoolableObject presentationPoolableObject,
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

        public override void OnPresentationEnabled()
        {
            base.OnPresentationEnabled();
            
            _presentation.SetAngle(0, shouldRandomize: true);
        }

        protected override void GetDestroyed()
        {
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
