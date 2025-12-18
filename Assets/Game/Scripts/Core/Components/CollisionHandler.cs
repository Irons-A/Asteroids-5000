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

        public event Action<int> OnDamageReceived;
        public event Action OnDestructionCalled;

        public void Configure(int damage, EntityAffiliation affiliation, EntityDurability durability)
        {
            Damage = damage;
            Affiliation = affiliation;
            Durability = durability;
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out CollisionHandler otherHandler)) return;
            
            if (otherHandler.Affiliation != this.Affiliation && 
                ShouldHandleCollision(otherHandler))
            {
                HandleCollision(otherHandler);
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
            
            if (otherHandler.Durability == EntityDurability.Fragile)
            {
                otherHandler.CallForDestruction();
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
