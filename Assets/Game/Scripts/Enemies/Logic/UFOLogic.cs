using System;
using Core.Components;
using Core.Configuration;
using Core.Configuration.Enemies;
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
        private readonly EnemyShootingSystem _shootingSystem;
        private readonly UFOSettings _settings;
        
        private UFOPresentation _presentation;
        private Transform _targetTransform;
        
        protected override EnemyType Type => EnemyType.UFO;
        
        public UFOLogic(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            SignalBus signalBus, EnemyShootingSystem shootingSystem, ParticleService  particleService)
        {
            _settings = configProvider.UFOSettingsRef;
            Physics = physics;
            HealthSystem = healthSystem;
            SignalBus = signalBus;
            _shootingSystem = shootingSystem;
            ParticleService  = particleService;
        }
        
        public void Configure(UFOPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            _presentation.OnTargetTransformChanged += SetTargetTransform;
            
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
            
            _shootingSystem.Configure(_settings.ProjectileDamage, _settings.FireRateInterval,
                _settings.ProjectileSpeed, _presentation, PoolableObjectType.UFOBullet,
                _presentation.Firepoints);
        }

        public void RotateTowardsPlayer()
        {
            if (_targetTransform == null) return;
            
            Vector3 direction = _targetTransform.position - _presentation.transform.position;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

            _presentation.transform.rotation = Quaternion.RotateTowards(_presentation.transform.rotation,
                targetRotation, _settings.RotationSpeed * Time.deltaTime);
        }
        
        public override void Move()
        {
            Physics.SetInstantVelocity(_settings.Speed);
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
            
            ParticleService.SpawnParticles(PoolableObjectType.ExplosionParticles, _presentation.transform.position);
            
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
