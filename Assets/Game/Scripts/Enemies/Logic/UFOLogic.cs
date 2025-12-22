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
        private UFOPresentation _presentation;
        private Transform _targetTransform;
        private EnemyShootingSystem _shootingSystem;
        
        protected override EnemyType Type => EnemyType.UFO;
        
        [Inject]
        private void Construct(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            SignalBus signalBus, EnemyShootingSystem shootingSystem)
        {
            Settings = configProvider.EnemySettingsRef;
            Physics = physics;
            HealthSystem = healthSystem;
            SignalBus = signalBus;
            _shootingSystem = shootingSystem;
        }
        
        public void Configure(UFOPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            _presentation.OnTargetTransformChanged += SetTargetTransform;
            
            Physics.SetMovableObject(_presentation);
            
            PoolableObject = presentationPoolableObject;
            
            CollisionHandler = collisionHandler;
            CollisionHandler.Configure(Settings.UFODamage, EntityAffiliation.Enemy, 
                EntityDurability.Piercing, shouldCauseRicochet: true);
            CollisionHandler.OnDamageReceived += HealthSystem.TakeDamage;
            CollisionHandler.OnDestructionCalled += GetDestroyed;
            CollisionHandler.OnRicochetCalled += Physics.ApplyRicochet;
            
            HealthSystem.Configure(Settings.UFOHealth, true);
            HealthSystem.OnHealthDepleted += GetDestroyed;
            
            _shootingSystem.Configure(Settings.UFODamage, Settings.UFOFireRateInterval,
                Settings.UFOProjectileSpeed, _presentation.transform, PoolableObjectType.UFOBullet,
                _presentation.Firepoints);
        }

        public void RotateTowardsPlayer()
        {
            if (_targetTransform == null) return;
            
            Vector3 direction = _targetTransform.position - _presentation.transform.position;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            _presentation.transform.rotation = Quaternion.RotateTowards(_presentation.transform.rotation,
                targetRotation, Settings.UFORotationSpeed * Time.deltaTime);
        }
        
        public override void Move()
        {
            Physics.SetInstantVelocity(Settings.UFOSpeed);
            Physics.ProcessPhysics();
        }

        public override void OnPresentationEnabled()
        {
            base.OnPresentationEnabled();
            _presentation.SetAngle(0, shouldRandomize:false, setAngleToPlayer: true);
            _shootingSystem.TryStartShooting();
        }

        protected override void GetDestroyed()
        {
            _shootingSystem.StopShooting();
            base.GetDestroyed();
        }

        private void SetTargetTransform(Transform targetTransform)
        {
            _targetTransform = targetTransform;
            _shootingSystem.SetTarget(_targetTransform);
            _shootingSystem.TryStartShooting();
        }
        
        public void Dispose()
        {
            _shootingSystem.StopShooting();
            
            if (HealthSystem != null) HealthSystem.OnHealthDepleted -= GetDestroyed;
            
            if (CollisionHandler != null)
            {
                CollisionHandler.OnDamageReceived -= HealthSystem.TakeDamage;
                CollisionHandler.OnDestructionCalled -= GetDestroyed;
                CollisionHandler.OnRicochetCalled -= Physics.ApplyRicochet;
            }

            if (_presentation != null)
            {
                _presentation.OnTargetTransformChanged -= SetTargetTransform;
            }
        }
    }
}
