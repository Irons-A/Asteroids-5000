using Codice.Client.Common.GameUI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Components;
using UnityEngine;
using Zenject;

namespace Core.Physics
{
    public class CustomPhysics
    {
        public const float BaseFriction = 0;
        public const float BaseObjectMass = 0; // Возвращаем 0

        private float _friction;
        private float _objectMass;

        private Transform _movableObjectTransform;
        private CollisionHandler _collisionHandler;

        private Vector2 _currentAcceleration;
        private Vector2 _currentVelocity;

        public float CurrentSpeed { get; private set; } = 0;

        public void SetMovableObject(MovableObject movableObject, float friction = BaseFriction,
            float objectMass = BaseObjectMass, CollisionHandler collisionHandler = null)
        {
            _movableObjectTransform = movableObject.transform;
            _friction = friction <= 0 ? BaseFriction : friction;
            _objectMass = objectMass <= 0 ? BaseObjectMass : objectMass;
            _collisionHandler = collisionHandler;

            _currentAcceleration = Vector2.zero;
            _currentVelocity = Vector2.zero;
        }

        public void ApplyAcceleration(float acceleration, float maxSpeed)
        {
            if (_movableObjectTransform == null) return;

            Vector2 direction = _movableObjectTransform.right;

            float effectiveAcceleration = acceleration;

            if (_objectMass > 0)
            {
                effectiveAcceleration = acceleration / (1 + _objectMass);
            }

            _currentAcceleration += direction * effectiveAcceleration;

            float currentSpeed = _currentVelocity.magnitude;

            if (currentSpeed > maxSpeed)
            {
                _currentVelocity = _currentVelocity.normalized * maxSpeed;
            }
        }

        public void ApplyDeceleration(float deceleration)
        {
            if (_movableObjectTransform == null) return;

            float effectiveDeceleration = deceleration;

            if (_objectMass > 0)
            {
                effectiveDeceleration = deceleration / (1 + _objectMass);
            }

            if (_currentVelocity.magnitude > effectiveDeceleration)
            {
                _currentVelocity -= _currentVelocity.normalized * effectiveDeceleration;
            }
            else
            {
                _currentVelocity = Vector2.zero;
            }
        }
        
        public void ProcessPhysics()
        {
            if (_movableObjectTransform == null) return;
            
            _currentVelocity += _currentAcceleration * Time.fixedDeltaTime;
            _currentAcceleration = Vector2.zero;
            
            CurrentSpeed = _currentVelocity.magnitude;
            
            if (_friction > 0)
            {
                ApplyFriction();
            }
            
            Vector2 movement = _currentVelocity * Time.fixedDeltaTime;
            
            _movableObjectTransform.position += new Vector3(movement.x, movement.y, 0);
        }

        public Vector2 GetVelocity()
        {
            return _currentVelocity;
        }
        
        public void ApplyRicochet(CollisionData collisionData)
        {
            // Нормаль должна быть направлена ОТ поверхности столкновения
            Vector2 normal = collisionData.normal.normalized;
            Vector2 velocity = _currentVelocity;
            Vector2 otherVelocity = collisionData.otherVelocity;
            
            // Рассчитываем относительную скорость
            Vector2 relativeVelocity = velocity - otherVelocity;
            
            // Проекция относительной скорости на нормаль
            float normalRelativeSpeed = Vector2.Dot(relativeVelocity, normal);
            
            // Если объекты удаляются друг от друга, не обрабатываем столкновение
            if (normalRelativeSpeed > 0)
            {
                return;
            }
            
            // Получаем коэффициенты восстановления и трения
            float restitution1 = _collisionHandler != null ? _collisionHandler.Restitution : 0.8f;
            float restitution2 = collisionData.otherRestitution;
            float effectiveRestitution = Mathf.Min(restitution1, restitution2); // Используем меньший коэффициент
            
            // Коэффициенты трения
            float friction1 = _collisionHandler != null ? _collisionHandler.Friction : 0.1f;
            float friction2 = collisionData.otherFriction;
            float effectiveFriction = (friction1 + friction2) * 0.5f;
            
            // Разлагаем скорость на нормальную и тангенциальную составляющие
            float normalSpeed = Vector2.Dot(velocity, normal);
            Vector2 normalVelocity = normal * normalSpeed;
            Vector2 tangentVelocity = velocity - normalVelocity;
            
            // Рассчитываем новую нормальную скорость с учетом другого объекта
            float impulseMagnitude = -(1 + effectiveRestitution) * normalRelativeSpeed;
            
            // Если у объектов есть масса, учитываем ее, но пока будем считать массы равными
            // (можно добавить поле массы в CollisionHandler)
            float thisMass = _objectMass > 0 ? _objectMass : 1;
            float otherMass = 1; // Предполагаем массу другого объекта = 1
            
            // Импульс
            Vector2 impulse = normal * impulseMagnitude;
            
            // Новая скорость = старая скорость + импульс/масса
            Vector2 newVelocity = velocity + impulse / thisMass;
            
            // Применяем трение к тангенциальной составляющей
            Vector2 newTangentVelocity = tangentVelocity * (1f - effectiveFriction);
            Vector2 newNormalVelocity = newVelocity - tangentVelocity + newTangentVelocity;
            
            // Устанавливаем новую скорость
            _currentVelocity = newNormalVelocity;
            
            // Поворачиваем объект в направлении движения
            if (newNormalVelocity.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(newNormalVelocity.y, newNormalVelocity.x) * Mathf.Rad2Deg;
                _movableObjectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public void SetInstantVelocity(float speed)
        {
            if (_movableObjectTransform == null) return;

            Vector2 direction;
            
            direction = _movableObjectTransform.right;
            
            _currentVelocity = direction * speed;
        }

        public void AddImpulse(Vector2 impulse)
        {
            if (_objectMass > 0)
            {
                _currentVelocity += impulse / _objectMass;
            }
            else
            {
                _currentVelocity += impulse;
            }
        }
        
        public void Stop()
        {
            _currentVelocity = Vector2.zero;
            _currentAcceleration = Vector2.zero;
        }
        
        private void ApplyFriction()
        {
            float effectiveFriction = _friction;

            if (_objectMass > 0)
            {
                effectiveFriction = _friction / (1 + _objectMass);
            }

            if (_currentVelocity.magnitude > effectiveFriction)
            {
                _currentVelocity -= _currentVelocity.normalized * effectiveFriction;
            }
            else
            {
                _currentVelocity = Vector2.zero;
            }
        }
    }
}
