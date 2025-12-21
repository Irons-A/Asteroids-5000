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
    public class UFOLogic : BaseEnemyLogic, IDisposable
    {
        protected override EnemyType Type => EnemyType.UFO;
        
        private UFOPresentation _presentation;
        
        [Inject]
        private void Construct(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            SignalBus signalBus)
        {
            _settings = configProvider.EnemySettingsRef;
            _physics = physics;
            _healthSystem = healthSystem;
            _signalBus = signalBus;
        }
        
        public void Configure(UFOPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            _presentation.OnAngleUpdated += ResetMovement;
            
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

        public void RotateTowardsPlayer()
        {
            if (_presentation.PlayerTransform == null) return;
            
            Vector3 direction = _presentation.PlayerTransform.position - _presentation.transform.position;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            _presentation.transform.rotation = Quaternion.RotateTowards(_presentation.transform.rotation,
                targetRotation, _settings.UFORotationSpeed * Time.deltaTime);

            Debug.Log($"PlayerTransform is {_presentation.PlayerTransform}");
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

        private void ResetMovement()
        {
            //_physics.SetInstantVelocity(_settings.BigAsteroidSpeed);
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

            if (_presentation != null)
            {
                _presentation.OnAngleUpdated -= ResetMovement;
            }
        }
    }
}
