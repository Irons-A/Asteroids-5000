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
using Random = UnityEngine.Random;

namespace Enemies.Logic
{
    public class BigAsteroidLogic : IInitializable, IDisposable
    {
        private EnemyType _type;
        private BigAsteroidPresentation _presentation;
        //private Transform _transform;
        private EnemySettings _settings;
        private CustomPhysics _physics;
        private PoolAccessProvider _objectPool;
        private PoolableObject _poolableObject;
        private HealthSystem _healthSystem; 
        private CollisionHandler _collisionHandler;
        //collisionHandler

        private int _minSmallAsteroidSpawnAmount;
        private int _maxSmallAsteroidSpawnAmount;

        [Inject]
        private void Construct(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem  healthSystem,
            PoolAccessProvider accessProvider)
        {
            _settings = configProvider.EnemySettingsRef;
            _physics = physics;
            _healthSystem = healthSystem;
            _objectPool = accessProvider;
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

        public void ProcessFixedUpdate()
        {
            _physics.SetInstantVelocity(_settings.BigAsteroidSpeed);
            _physics.ProcessPhysics();
        }

        public void Dispose()
        {
            _healthSystem.OnHealthDepleted -= GetDestroyed;
            _collisionHandler.OnDamageReceived -= _healthSystem.TakeDamage;
        }

        public void Configure(BigAsteroidPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            _physics.SetMovableObject(_presentation);
            
            _poolableObject = presentationPoolableObject;
            
            _collisionHandler = collisionHandler;
            _collisionHandler.OnDamageReceived += _healthSystem.TakeDamage;
            
            _healthSystem.Configure(_settings.BigAsteroidHealth, true);
            _healthSystem.OnHealthDepleted += GetDestroyed;
        }

        public void OnPresentationEnabled()
        {
            _healthSystem.RestoreHealth();
        }


        private void GetDestroyed()
        {
            //SpawnSmallAsteroids();
            //SignalBus
            
            _poolableObject.Despawn();
        }

        private void SpawnSmallAsteroids()
        {
            int asteroidsToSpawn = Random.Range(_minSmallAsteroidSpawnAmount, _maxSmallAsteroidSpawnAmount);

            for (int i = 0; i < asteroidsToSpawn; i++)
            {
                PoolableObject smallAsteroid = _objectPool.GetFromPool(PoolableObjectType.SmallAsteroid);
                
            }
        }
    }
}
