using System;
using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Configuration;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using Enemies.Presentation;
using UnityEngine;
using Zenject;

namespace Enemies.Logic
{
    public class SmallAsteroidLogic : BaseEnemyLogic, IDisposable
    {
        protected override EnemyType Type => EnemyType.SmallAsteroid;
        
        private SmallAsteroidPresentation _presentation;
        
        [Inject]
        private void Construct(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            SignalBus signalBus)
        {
            _settings = configProvider.EnemySettingsRef;
            _physics = physics;
            _healthSystem = healthSystem;
            _signalBus = signalBus;
        }
        
        public void Configure(SmallAsteroidPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            
            _physics.SetMovableObject(_presentation);
            
            _poolableObject = presentationPoolableObject;
            
            _collisionHandler = collisionHandler;
            _collisionHandler.Configure(_settings.SmallAsteroidDamage, EntityAffiliation.Enemy, 
                EntityDurability.Piercing, shouldCauseRicochet: true);
            _collisionHandler.OnDamageReceived += _healthSystem.TakeDamage;
            _collisionHandler.OnDestructionCalled += GetDestroyed;
            _collisionHandler.OnRicochetCalled += _physics.ApplyRicochet;
            
            _healthSystem.Configure(_settings.SmallAsteroidHealth, true);
            _healthSystem.OnHealthDepleted += GetDestroyed;
        }
        
        public override void Move()
        {
            _physics.SetInstantVelocity(_settings.BigAsteroidSpeed);
            _physics.ProcessPhysics();
        }

        public override void OnPresentationEnabled()
        {
            base.OnPresentationEnabled();
            _presentation.SetAngle(0, true);
        }
        
        public void Dispose()
        {
            if (_healthSystem != null) _healthSystem.OnHealthDepleted -= GetDestroyed;
            
            if (_collisionHandler != null)
            {
                _collisionHandler.OnDamageReceived -= _healthSystem.TakeDamage;
                _collisionHandler.OnDestructionCalled -= GetDestroyed;
                _collisionHandler.OnRicochetCalled -= _physics.ApplyRicochet;
            }
        }
    }
}
