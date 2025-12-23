using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Systems;
using Core.Systems.ObjectPools;
using UnityEngine;

namespace Core.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionHandler : MonoBehaviour
    {
        [field: SerializeField] public int Damage { get; private set; } = 0;
        [field: SerializeField] public EntityAffiliation Affiliation { get; private set; }
        [field: SerializeField] public EntityDurability Durability { get; private set; }
        [field: SerializeField] public bool ShouldCauseRicochet { get; private set; } = false;
        [field: SerializeField] public bool ShouldProcessCollisions { get; private set; } = true;

        public event Action<int> OnDamageReceived;
        public event Action OnDestructionCalled;
        public event Action OnRicochetCalled;

        public void Configure(int damage, EntityAffiliation affiliation, EntityDurability durability, 
            bool shouldCauseRicochet = false)
        {
            Damage = damage;
            Affiliation = affiliation;
            Durability = durability;
            ShouldCauseRicochet = shouldCauseRicochet;
        }

        public void DealDamage(int damage)
        {
            if (damage > 0)
            {
                OnDamageReceived?.Invoke(damage);
            }
        }

        public void CallForDestruction()
        {
            OnDestructionCalled?.Invoke();
        }

        public void CallForRicochet()
        {
            OnRicochetCalled?.Invoke();
        }

        public void SetShouldProcessCollisions(bool value)
        {
            ShouldProcessCollisions = value;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out CollisionHandler otherHandler) == false) return;

            if (ShouldProcessCollisions && otherHandler.ShouldProcessCollisions)
            {
                if (otherHandler.Affiliation != this.Affiliation && 
                    ShouldHandleCollision(otherHandler))
                {
                    HandleCollision(otherHandler);
                }
            }
        }

        private bool ShouldHandleCollision(CollisionHandler other)
        {
            return this.GetInstanceID() > other.GetInstanceID();
        }

        private void HandleCollision(CollisionHandler otherHandler)
        {
            otherHandler.DealDamage(Damage);
            
            this.DealDamage(otherHandler.Damage);
            
            if (otherHandler.ShouldCauseRicochet)
            {
                CallForRicochet();
            }
            
            if (otherHandler.Durability == EntityDurability.Fragile)
            {
                otherHandler.CallForDestruction();
            }
            
            if (ShouldCauseRicochet)
            {
                otherHandler.CallForRicochet();
            }
            
            if (Durability == EntityDurability.Fragile)
            {
                CallForDestruction();
            }
        }
        
        private void OnDestroy()
        {
            OnDamageReceived = null;
            OnDestructionCalled = null;
        }
    }
}
