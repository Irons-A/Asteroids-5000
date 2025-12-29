using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Physics;
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
        
        // Физические свойства для столкновений
        [SerializeField] private float _restitution = 0.8f; // Коэффициент восстановления (0-1)
        [SerializeField] private float _friction = 0.1f; // Коэффициент трения

        public event Action<int> OnDamageReceived;
        public event Action OnDestructionCalled;
        public event Action<CollisionData> OnRicochetCalled;

        public float Restitution => _restitution;
        public float Friction => _friction;

        public void Configure(int damage, EntityAffiliation affiliation, EntityDurability durability, 
            bool shouldCauseRicochet = false, float restitution = 0.8f, float friction = 0.1f)
        {
            Damage = damage;
            Affiliation = affiliation;
            Durability = durability;
            ShouldCauseRicochet = shouldCauseRicochet;
            _restitution = restitution;
            _friction = friction;
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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out CollisionHandler otherHandler) == false) return;

            if (ShouldProcessCollisions && otherHandler.ShouldProcessCollisions)
            {
                if (otherHandler.Affiliation != this.Affiliation && 
                    ShouldHandleCollision(otherHandler))
                {
                    HandleCollision(otherHandler, other);
                }
            }
        }

        private bool ShouldHandleCollision(CollisionHandler other)
        {
            return this.GetInstanceID() > other.GetInstanceID();
        }

        private void HandleCollision(CollisionHandler otherHandler, Collider2D otherCollider)
        {
            // ВЫЧИСЛЯЕМ ОБЩУЮ НОРМАЛЬ ДЛЯ СТОЛКНОВЕНИЯ
            Vector2 collisionNormal = CalculateCollisionNormal(otherCollider);
            Vector2 collisionPoint = CalculateCollisionPoint(otherCollider);
            
            // Получаем скорость другого объекта (если есть)
            Vector2 otherVelocity = Vector2.zero;
            if (otherHandler.TryGetComponent<MovableObject>(out var otherMovable))
            {
                //otherVelocity = otherMovable.CustomPhysics?.GetVelocity() ?? Vector2.zero;
                otherVelocity =  Vector2.zero;
            }
            
            // Создаем CollisionData для ЭТОГО объекта
            // Нормаль направлена ОТ другого объекта
            var collisionDataForThis = new CollisionData(
                collisionPoint, 
                -collisionNormal, // Нормаль от другого объекта к этому
                otherHandler.Restitution,
                otherHandler.Friction,
                otherVelocity
            );
            
            // Создаем CollisionData для ДРУГОГО объекта
            // Нормаль должна быть противоположной - ОТ этого объекта к другому
            Vector2 thisVelocity = Vector2.zero;
            if (TryGetComponent<MovableObject>(out var thisMovable))
            {
                //thisVelocity = thisMovable.CustomPhysics?.GetVelocity() ?? Vector2.zero;
                thisVelocity = Vector2.zero;
            }
            
            var collisionDataForOther = new CollisionData(
                collisionPoint, 
                collisionNormal, // Инвертируем нормаль для другого объекта
                this.Restitution,
                this.Friction,
                thisVelocity
            );
            
            otherHandler.DealDamage(Damage);
            this.DealDamage(otherHandler.Damage);
            
            // Вызываем рикошет для обоих объектов с РАЗНЫМИ нормалями
            if (otherHandler.ShouldCauseRicochet)
            {
                otherHandler.CallForRicochet(collisionDataForOther); // Передаем данные с ИНВЕРТИРОВАННОЙ нормалью
            }
            
            if (otherHandler.Durability == EntityDurability.Fragile)
            {
                otherHandler.CallForDestruction();
            }
            
            if (ShouldCauseRicochet)
            {
                CallForRicochet(collisionDataForThis); // Передаем данные с НОРМАЛЬЮ
            }
            
            if (Durability == EntityDurability.Fragile)
            {
                CallForDestruction();
            }
        }
        
        private Vector2 CalculateCollisionNormal(Collider2D otherCollider)
        {
            // Для круглых коллайдеров нормаль = направление ОТ другого объекта к этому
            Vector2 direction = ((Vector2)transform.position - (Vector2)otherCollider.transform.position).normalized;
            
            // Уточняем через ClosestPoint для более точного расчета
            Collider2D thisCollider = GetComponent<Collider2D>();
            if (thisCollider != null)
            {
                Vector2 closestPointOnOther = otherCollider.ClosestPoint(transform.position);
                Vector2 closestPointOnThis = thisCollider.ClosestPoint(otherCollider.transform.position);
                
                // Нормаль направлена от точки на другом объекте к точке на этом объекте
                direction = (closestPointOnThis - closestPointOnOther).normalized;
                
                // Если векторы почти противоположны, используем простой расчет
                if (direction.magnitude < 0.1f)
                {
                    direction = ((Vector2)transform.position - (Vector2)otherCollider.transform.position).normalized;
                }
            }
            
            return direction;
        }
        
        private Vector2 CalculateCollisionPoint(Collider2D otherCollider)
        {
            // Для круглых коллайдеров используем середину между центрами
            // Для большей точности можно использовать точки касания, но это сложнее
            Vector2 thisPos = transform.position;
            Vector2 otherPos = otherCollider.transform.position;
            
            // Если коллайдеры круглые, можно использовать линейную интерполяцию на основе радиусов
            CircleCollider2D thisCircle = GetComponent<CircleCollider2D>();
            CircleCollider2D otherCircle = otherCollider as CircleCollider2D;
            
            if (thisCircle != null && otherCircle != null)
            {
                // Для двух кругов точка столкновения лежит на линии между центрами
                float thisRadius = thisCircle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
                float otherRadius = otherCircle.radius * Mathf.Max(otherCollider.transform.lossyScale.x, otherCollider.transform.lossyScale.y);
                float totalRadius = thisRadius + otherRadius;
                float ratio = thisRadius / totalRadius;
                
                return Vector2.Lerp(otherPos, thisPos, ratio);
            }
            
            // Для других типов коллайдеров используем среднюю точку
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
