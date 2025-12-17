using System;
using System.Collections;
using System.Collections.Generic;
using Core.Systems;
using Core.Systems.ObjectPools;
using UnityEngine;

namespace Core.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionHandler : MonoBehaviour, IDisposable
    {
        [field: SerializeField] public int Damage { get; private set; } = 0;
        [field: SerializeField] public EntityAffiliation Affiliation { get; private set; }
        [field: SerializeField] public EntityDurability Durability { get; private set; }
        
        private PoolableObject _poolableObject;

        public event Action<int> OnDamageReceived;

        public void Configure(int damage, EntityAffiliation affiliation, EntityDurability durability)
        {
            Damage = damage;
            Affiliation = affiliation;
            Durability = durability;
        }
        
        private void Awake()
        {
            if (TryGetComponent(out PoolableObject poolableObject))
            {
                _poolableObject = poolableObject;
            }
        }

        public void Dispose()
        {
            OnDamageReceived = null;
        }

        public void DealDamage(int damage)
        {
            if (damage > 0)
            {
                OnDamageReceived?.Invoke(damage);
            }
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
                if (otherHandler.TryGetComponent(out PoolableObject otherPoolableObject))
                {
                    otherPoolableObject.Despawn();
                }
                else
                {
                    Destroy(otherHandler.gameObject);
                }
            }
            
            if (Durability == EntityDurability.Fragile)
            {
                if (_poolableObject != null)
                {
                    _poolableObject.Despawn();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
