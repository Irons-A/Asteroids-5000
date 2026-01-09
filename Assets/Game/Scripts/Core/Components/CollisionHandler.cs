using System;
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
        [SerializeField] private int _damage = 0;
        [SerializeField] private EntityAffiliation _affiliation;
        [SerializeField] private EntityDurability _durability;
        [SerializeField] private bool _shouldCauseRicochet = false;
        [SerializeField] private bool _shouldReceiveRicochet = true;
        [SerializeField] private bool _shouldProcessCollisions = true;
        
        private CustomPhysics _customPhysics;
        private ParticleService _particleService;

        public event Action<int> OnDamageReceived;
        public event Action OnDestructionCalled;
        public event Action<CollisionData> OnRicochetCalled;
        
        public int Damage => _damage;
        public EntityAffiliation Affiliation => _affiliation;
        public EntityDurability Durability =>  _durability;
        public bool ShouldCauseRicochet => _shouldCauseRicochet;
        public bool ShouldReceiveRicochet => _shouldReceiveRicochet;
        public bool ShouldProcessCollisions => _shouldProcessCollisions;
        
        public float Restitution => _customPhysics != null ? _customPhysics.Restitution : 0;
        public float Friction => _customPhysics != null ? _customPhysics.Friction : 0;
        public Vector2 CurrentVelocity => _customPhysics != null ? _customPhysics.CurrentVelocity : Vector2.zero;
        public float Mass => _customPhysics != null ? _customPhysics.ObjectMass : 1;

        [Inject]
        private void Construct(ParticleService particleService)
        {
            _particleService = particleService;
        }
        
        public void Configure(int damage, EntityAffiliation affiliation, EntityDurability durability, 
            bool shouldCauseRicochet = false, CustomPhysics customPhysics = null)
        {
            _damage = damage;
            _affiliation = affiliation;
            _durability = durability;
            _shouldCauseRicochet = shouldCauseRicochet;
            _customPhysics = customPhysics;
        }
        
        public void SetShouldProcessCollisions(bool value)
        {
            _shouldProcessCollisions = value;
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

            if (_shouldProcessCollisions && otherHandler.ShouldProcessCollisions)
            {
                if (otherHandler.Affiliation != _affiliation && 
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
        
            otherHandler.DealDamage(_damage);
            DealDamage(otherHandler.Damage);

            bool shouldSpawnCollisionParticles = false;
            
            if (otherHandler.ShouldCauseRicochet && _shouldReceiveRicochet)
            {
                CallForRicochet(collisionDataForThis);
                shouldSpawnCollisionParticles = true;
            }
            
            if (_shouldCauseRicochet && otherHandler.ShouldReceiveRicochet)
            {
                otherHandler.CallForRicochet(collisionDataForOther);
                shouldSpawnCollisionParticles = true;
            }

            if (shouldSpawnCollisionParticles)
            {
                Vector2 collisionPoint = CalculateCollisionPoint(otherCollider);

                _particleService.SpawnParticles(PoolableObjectType.CollisionParticles, collisionPoint);
            }
        
            if (otherHandler.Durability == EntityDurability.Fragile)
            {
                otherHandler.CallForDestruction();
            }
        
            if (_durability == EntityDurability.Fragile)
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
