using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Physics;
using Core.Systems;
using Core.Systems.ObjectPools;
using UnityEngine;
using Zenject;

namespace Core.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionHandler : MonoBehaviour
    {
        [field: SerializeField] public int Damage { get; private set; } = 0;
        [field: SerializeField] public EntityAffiliation Affiliation { get; private set; }
        [field: SerializeField] public EntityDurability Durability { get; private set; }
        [field: SerializeField] public bool ShouldCauseRicochet { get; private set; } = false;
        [field: SerializeField] public bool ShouldReceiveRicochet { get; private set; } = true;
        [field: SerializeField] public bool ShouldProcessCollisions { get; private set; } = true;

        private CustomPhysics _customPhysics;
        private PoolAccessProvider _poolAccessProvider;

        public event Action<int> OnDamageReceived;
        public event Action OnDestructionCalled;
        public event Action<CollisionData> OnRicochetCalled;

        public float Restitution => _customPhysics != null ? _customPhysics.Restitution : 0;
        public float Friction => _customPhysics != null ? _customPhysics.Friction : 0;
        public Vector2 CurrentVelocity => _customPhysics != null ? _customPhysics.CurrentVelocity : Vector2.zero;
        public float Mass => _customPhysics != null ? _customPhysics.ObjectMass : 1;

        [Inject]
        private void Construct(PoolAccessProvider poolAccessProvider)
        {
            _poolAccessProvider = poolAccessProvider;
        }
        
        public void Configure(int damage, EntityAffiliation affiliation, EntityDurability durability, 
            bool shouldCauseRicochet = false, CustomPhysics customPhysics = null)
        {
            Damage = damage;
            Affiliation = affiliation;
            Durability = durability;
            ShouldCauseRicochet = shouldCauseRicochet;
            _customPhysics = customPhysics;
        }
        
        public void SetShouldProcessCollisions(bool value)
        {
            ShouldProcessCollisions = value;
        }

        private void DealDamage(int damage)
        {
            if (damage > 0)
            {
                OnDamageReceived?.Invoke(damage);
            }
        }

        private void CallForDestruction()
        {
            OnDestructionCalled?.Invoke();
        }

        private void CallForRicochet(CollisionData collisionData)
        {
            OnRicochetCalled?.Invoke(collisionData);
        }

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (otherCollider.TryGetComponent(out CollisionHandler otherHandler) == false) return;

            if (ShouldProcessCollisions && otherHandler.ShouldProcessCollisions)
            {
                if (otherHandler.Affiliation != this.Affiliation && 
                    ShouldHandleCollision(otherHandler))
                {
                    HandleCollision(otherHandler, otherCollider);
                }
            }
        }

        private bool ShouldHandleCollision(CollisionHandler other)
        {
            return this.GetInstanceID() > other.GetInstanceID();
        }

        private void HandleCollision(CollisionHandler otherHandler, Collider2D otherCollider)
        {
            Vector2 collisionNormal = CalculateCollisionNormal(otherCollider);
        
            var collisionDataForThis = new CollisionData(
                collisionNormal * -1,
                otherHandler.Restitution,
                otherHandler.Friction,
                otherHandler.Mass,
                otherHandler.CurrentVelocity
            );
        
            var collisionDataForOther = new CollisionData(
                collisionNormal,
                Restitution,
                Friction,
                Mass,
                CurrentVelocity
            );
        
            otherHandler.DealDamage(Damage);
            this.DealDamage(otherHandler.Damage);

            bool shouldSpawnCollisionParticles = false;
            
            if (otherHandler.ShouldCauseRicochet && this.ShouldReceiveRicochet)
            {
                CallForRicochet(collisionDataForThis);
                shouldSpawnCollisionParticles = true;
            }
            
            if (ShouldCauseRicochet && otherHandler.ShouldReceiveRicochet)
            {
                otherHandler.CallForRicochet(collisionDataForOther);
                shouldSpawnCollisionParticles = true;
            }

            if (shouldSpawnCollisionParticles)
            {
                Vector2 collisionPoint = CalculateCollisionPoint(otherCollider);

                PoolableObject collisionParticles = _poolAccessProvider.GetFromPool(PoolableObjectType.CollisionParticles);
                collisionParticles.transform.position = collisionPoint;
            }
        
            if (otherHandler.Durability == EntityDurability.Fragile)
            {
                otherHandler.CallForDestruction();
            }
        
            if (Durability == EntityDurability.Fragile)
            {
                CallForDestruction();
            }
        }
        
        private Vector2 CalculateCollisionNormal(Collider2D otherCollider)
        {
            Vector2 direction = ((Vector2)transform.position - (Vector2)otherCollider.transform.position).normalized;
            
            Collider2D thisCollider = GetComponent<Collider2D>();
            
            if (thisCollider != null)
            {
                Vector2 closestPointOnOther = otherCollider.ClosestPoint(transform.position);
                Vector2 closestPointOnThis = thisCollider.ClosestPoint(otherCollider.transform.position);
                
                direction = (closestPointOnThis - closestPointOnOther).normalized;
                
                if (direction.magnitude < 0.1f)
                {
                    direction = ((Vector2)transform.position - (Vector2)otherCollider.transform.position).normalized;
                }
            }
            
            return direction;
        }
        
        private Vector2 CalculateCollisionPoint(Collider2D otherCollider)
        {
            Vector2 thisPos = transform.position;
            Vector2 otherPos = otherCollider.transform.position;
            
            CircleCollider2D thisCircle = GetComponent<CircleCollider2D>();
            CircleCollider2D otherCircle = otherCollider as CircleCollider2D;
            
            if (thisCircle != null && otherCircle != null)
            {
                float thisRadius = thisCircle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                float otherRadius = otherCircle.radius * Mathf.Max(otherCollider.transform.lossyScale.x,
                    otherCollider.transform.lossyScale.y);
                float totalRadius = thisRadius + otherRadius;
                float ratio = thisRadius / totalRadius;
                
                return Vector2.Lerp(otherPos, thisPos, ratio);
            }
            
            return Vector2.Lerp(thisPos, otherPos, 0.5f);
        }
        
        private void OnDestroy()
        {
            OnDamageReceived = null;
            OnDestructionCalled = null;
            OnRicochetCalled = null;
        }
    }
}
