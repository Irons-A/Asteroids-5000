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
            Settings = configProvider.EnemySettingsRef;
            Physics = physics;
            HealthSystem = healthSystem;
            SignalBus = signalBus;
        }
        
        public void Configure(SmallAsteroidPresentation presentation, PoolableObject presentationPoolableObject,
            CollisionHandler collisionHandler)
        {
            _presentation = presentation;
            
            Physics.SetMovableObject(_presentation);
            
            PoolableObject = presentationPoolableObject;
            
            CollisionHandler = collisionHandler;
            CollisionHandler.Configure(Settings.SmallAsteroidDamage, EntityAffiliation.Enemy, 
                EntityDurability.Piercing, shouldCauseRicochet: true);
            CollisionHandler.OnDamageReceived += HealthSystem.TakeDamage;
            CollisionHandler.OnDestructionCalled += GetDestroyed;
            CollisionHandler.OnRicochetCalled += Physics.ApplyRicochet;
            
            HealthSystem.Configure(Settings.SmallAsteroidHealth, true);
            HealthSystem.OnHealthDepleted += GetDestroyed;
        }
        
        public override void Move()
        {
            Physics.SetInstantVelocity(Settings.SmallAsteroidSpeed);
            Physics.ProcessPhysics();
        }

        public override void OnPresentationEnabled()
        {
            base.OnPresentationEnabled();
            _presentation.SetAngle(0, shouldRandomize: true);
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
