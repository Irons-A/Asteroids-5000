using System;
using System.Collections;
using System.Collections.Generic;
using Core.Configuration;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using Enemies.Presentation;
using UnityEngine;
using Zenject;

namespace Enemies.Logic
{
    public class BigAsteroidLogic : IFixedTickable, IInitializable, IDisposable
    {
        private EnemyType _type;
        private BigAsteroidPresentation _presentation;
        private Transform _transform;
        private EnemySettings _settings;
        private CustomPhysics _physics;
        private UniversalObjectPool _objectPool;
        private PoolableObject _poolableObject;
        private HealthSystem _healthSystem;
        //collisionHandler

        [Inject]
        private void Construct(JsonConfigProvider configProvider, Transform enemyTransform, 
            CustomPhysics physics, UniversalObjectPool objectPool)
        {
            _settings = configProvider.EnemySettingsRef;
            
            _presentation = GameObject.GetComponent<BigAsteroidPresentation>();
            _transform = presentation.transform;
            
            
            _physics = physics;
            
            _objectPool = objectPool;

            _poolableObject = _presentation.GetComponent<PoolableObject>();
        }
        
        public void Initialize()
        {
            _physics.SetMovableObject(_presentation);
            _physics.SetInstantVelocity(_settings.BigAsteroidSpeed);
            
            _healthSystem.OnHealthDepleted += GetDestroyed;
            _presentation.OnEnabled += OnPresentationEnabled;
        }

        public void FixedTick()
        {
            _physics.ProcessPhysics();
        }

        public void Dispose()
        {
            _healthSystem.OnHealthDepleted -= GetDestroyed;
            _presentation.OnEnabled -= OnPresentationEnabled;
        }

        private void OnPresentationEnabled()
        {
            _healthSystem.Configure(_settings.BigAsteroidHealth, true);
        }


        private void GetDestroyed()
        {
            SpawnSmallAsteroids();
            
            _poolableObject.Despawn();
        }

        private void SpawnSmallAsteroids()
        {
            
        }
    }
}
