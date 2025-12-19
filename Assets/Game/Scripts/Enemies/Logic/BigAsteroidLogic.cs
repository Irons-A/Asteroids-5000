using System;
using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Configuration;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using Enemies.Presentation;
using Enemies.Signals;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Enemies.Logic
{
    public class BigAsteroidLogic : BaseEnemyLogic, IInitializable, IDisposable
    {
        protected override EnemyType Type => EnemyType.BigAsteroid;
        
        private PoolAccessProvider _objectPool;
        private BigAsteroidPresentation _presentation;

        private int _minSmallAsteroidSpawnAmount;
        private int _maxSmallAsteroidSpawnAmount;

        [Inject]
        private void Construct(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem  healthSystem,
            PoolAccessProvider accessProvider, SignalBus signalBus)
        {
            _settings = configProvider.EnemySettingsRef;
            _physics = physics;
            _healthSystem = healthSystem;
            _objectPool = accessProvider;
            _signalBus = signalBus;
        }
        
        public void Configure(BigAsteroidPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            _presentation.OnAngleUpdated += ResetMovement;
            
            _physics.SetMovableObject(_presentation);
            
            _poolableObject = presentationPoolableObject;
            
            _collisionHandler = collisionHandler;
            _collisionHandler.Configure(_settings.BigAsteroidDamage, EntityAffiliation.Enemy, 
                EntityDurability.Piercing, shouldCauseRicochet: true);
            _collisionHandler.OnDamageReceived += _healthSystem.TakeDamage;
            _collisionHandler.OnDestructionCalled += GetDestroyed;
            _collisionHandler.OnRicochetCalled += _physics.ApplyRicochet;
            
            _healthSystem.Configure(_settings.BigAsteroidHealth, true);
            _healthSystem.OnHealthDepleted += GetDestroyed;
        }
        
        public void Initialize()
        {
            _minSmallAsteroidSpawnAmount = _settings.MinSmallAsteroidSpawnAmount;
            _maxSmallAsteroidSpawnAmount = _settings.MaxSmallAsteroidSpawnAmount;

            if (_minSmallAsteroidSpawnAmount > _maxSmallAsteroidSpawnAmount)
            {
                _minSmallAsteroidSpawnAmount = _maxSmallAsteroidSpawnAmount;
            }
        }

        public override void Move()
        {
            _physics.ProcessPhysics();
        }

        protected override void GetDestroyed()
        {
            //SpawnSmallAsteroids();
            
            base.GetDestroyed();
        }

        private void ResetMovement()
        {
            _physics.SetInstantVelocity(_settings.BigAsteroidSpeed);
        }

        private void SpawnSmallAsteroids()
        {
            int asteroidsToSpawn = Random.Range(_minSmallAsteroidSpawnAmount, _maxSmallAsteroidSpawnAmount);

            for (int i = 0; i < asteroidsToSpawn; i++)
            {
                PoolableObject smallAsteroid = _objectPool.GetFromPool(PoolableObjectType.SmallAsteroid);
                
                _signalBus.TryFire(new EnemySpawnedSignal());
            }
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
