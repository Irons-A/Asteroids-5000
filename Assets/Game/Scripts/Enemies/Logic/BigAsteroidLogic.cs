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
        //collisionHandler
        
        private bool _isConfigured = false;

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

        }

        public void ProcessFixedUpdate()
        {
            _physics.SetInstantVelocity(_settings.BigAsteroidSpeed);
            _physics.ProcessPhysics();
        }

        public void Dispose()
        {
            _healthSystem.OnHealthDepleted -= GetDestroyed;
        }

        public void Configure(BigAsteroidPresentation presentation, PoolableObject presentationPoolableObject)
        {
            _presentation = presentation;
            _physics.SetMovableObject(_presentation);
            
            _poolableObject = presentationPoolableObject;
            
            _healthSystem.Configure(_settings.BigAsteroidHealth, true);
            _healthSystem.OnHealthDepleted += GetDestroyed;
            
            _isConfigured = true;
        }

        public void OnPresentationEnabled()
        {
            _healthSystem.RestoreHealth();
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
