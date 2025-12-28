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
    public class BigAsteroidLogic : BaseEnemyLogic, IDisposable
    {
        private PoolAccessProvider _objectPool;
        private BigAsteroidPresentation _presentation;

        private int _minSmallAsteroidSpawnAmount;
        private int _maxSmallAsteroidSpawnAmount;
        
        protected override EnemyType Type => EnemyType.BigAsteroid;
        
        public BigAsteroidLogic(JsonConfigProvider configProvider, CustomPhysics physics, HealthSystem healthSystem,
            PoolAccessProvider accessProvider, SignalBus signalBus)
        {
            Settings = configProvider.EnemySettingsRef;
            Physics = physics;
            HealthSystem = healthSystem;
            _objectPool = accessProvider;
            SignalBus = signalBus;
        }
        
        public void Configure(BigAsteroidPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            
            Physics.SetMovableObject(_presentation);
            
            PoolableObject = presentationPoolableObject;
            
            CollisionHandler = collisionHandler;
            CollisionHandler.Configure(Settings.BigAsteroidDamage, EntityAffiliation.Enemy, 
                EntityDurability.Piercing, shouldCauseRicochet: true);
            CollisionHandler.OnDamageReceived += HealthSystem.TakeDamage;
            CollisionHandler.OnDestructionCalled += GetDestroyed;
            CollisionHandler.OnRicochetCalled += Physics.ApplyRicochet;
            
            HealthSystem.Configure(Settings.BigAsteroidHealth, true);
            HealthSystem.OnHealthDepleted += GetDestroyed;
        }

        public override void Move()
        {
            Physics.SetInstantVelocity(Settings.BigAsteroidSpeed);
            Physics.ProcessPhysics();
        }

        protected override void GetDestroyed()
        {
            SpawnSmallAsteroids();
            
            base.GetDestroyed();
        }

        private void SpawnSmallAsteroids()
        {
            int asteroidsToSpawn = Random.Range(Settings.MinSmallAsteroidSpawnAmount,
                Settings.MaxSmallAsteroidSpawnAmount + 1);
            
            for (int i = 0; i < asteroidsToSpawn; i++)
            {
                PoolableObject smallAsteroid = _objectPool.GetFromPool(PoolableObjectType.SmallAsteroid);
                
                smallAsteroid.transform.position = _presentation.transform.position;
                
                SignalBus.TryFire(new EnemySpawnedSignal());
            }
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
