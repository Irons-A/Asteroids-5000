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
        private const float BaseFriction = 0;
        private const float BaseObjectMass = 1;
        private const float BaseRestitution = 0.7f;

        private Transform _movableObjectTransform;

        private Vector2 _currentAcceleration;
        
        public float ObjectMass { get; private set; }
        public float Friction { get; private set; }
        public float Restitution { get; private set; }
        public Vector2 CurrentVelocity { get; private set; }
        public float CurrentSpeed { get; private set; }

        public void SetMovableObject(MovableObject movableObject, float friction = BaseFriction,
            float objectMass = BaseObjectMass, float restitution = BaseRestitution)
        {
            _movableObjectTransform = movableObject.transform;
            Friction = friction <= 0 ? BaseFriction : friction;
            ObjectMass = objectMass <= 0 ? BaseObjectMass : objectMass;
            Restitution = restitution <= 0 ? BaseRestitution : restitution;

            _currentAcceleration = Vector2.zero;
            CurrentVelocity = Vector2.zero;
        }

        public void ApplyAcceleration(float acceleration, float maxSpeed)
        {
            if (_movableObjectTransform == null) return;

            Vector2 direction = _movableObjectTransform.right;

            float effectiveAcceleration = acceleration;

            if (ObjectMass > 0)
            {
                effectiveAcceleration = acceleration / (1 + ObjectMass);
            }

            _currentAcceleration += direction * effectiveAcceleration;

            float currentSpeed = CurrentVelocity.magnitude;

            if (currentSpeed > maxSpeed)
            {
                CurrentVelocity = CurrentVelocity.normalized * maxSpeed;
            }
        }

        public void ApplyDeceleration(float deceleration)
        {
            if (_movableObjectTransform == null) return;

            float effectiveDeceleration = deceleration;

            if (ObjectMass > 0)
            {
                effectiveDeceleration = deceleration / (1 + ObjectMass);
            }

            if (CurrentVelocity.magnitude > effectiveDeceleration)
            {
                CurrentVelocity -= CurrentVelocity.normalized * effectiveDeceleration;
            }
            else
            {
                CurrentVelocity = Vector2.zero;
            }
        }
        
        public void ProcessPhysics()
        {
            if (_movableObjectTransform == null) return;
            
            CurrentVelocity += _currentAcceleration * Time.fixedDeltaTime;
            _currentAcceleration = Vector2.zero;
            
            CurrentSpeed = CurrentVelocity.magnitude;
            
            if (Friction > 0)
            {
                ApplyFriction();
            }
            
            Vector2 movement = CurrentVelocity * Time.fixedDeltaTime;
            
            _movableObjectTransform.position += new Vector3(movement.x, movement.y, 0);
        }
        
        public void ApplyRicochet(CollisionData collisionData)
        {
            Vector2 normal = collisionData.normal.normalized;
            Vector2 velocity = CurrentVelocity;
            Vector2 otherVelocity = collisionData.otherVelocity;
            
            Vector2 relativeVelocity = velocity - otherVelocity;
            
            float normalRelativeSpeed = Vector2.Dot(relativeVelocity, normal);
            
            if (normalRelativeSpeed > 0)
            {
                return;
            }
            
            float restitution1 = Restitution;
            float restitution2 = collisionData.otherRestitution;
            float effectiveRestitution = Mathf.Min(restitution1, restitution2);
            
            float friction1 = Friction;
            float friction2 = collisionData.otherFriction;
            float effectiveFriction = (friction1 + friction2) * 0.5f;
            
            float normalSpeed = Vector2.Dot(velocity, normal);
            Vector2 normalVelocity = normal * normalSpeed;
            Vector2 tangentVelocity = velocity - normalVelocity;
            
            float impulseMagnitude = -(1 + effectiveRestitution) * normalRelativeSpeed;
            
            float thisMass = ObjectMass > 0 ? ObjectMass : 1;
            
            Vector2 impulse = normal * impulseMagnitude;
            
            Vector2 newVelocity = velocity + impulse / thisMass;
            
            Vector2 newTangentVelocity = tangentVelocity * (1f - effectiveFriction);
            Vector2 newNormalVelocity = newVelocity - tangentVelocity + newTangentVelocity;
            
            CurrentVelocity = newNormalVelocity;
            
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
            
            CurrentVelocity = direction * speed;
        }

        public void AddImpulse(Vector2 impulse)
        {
            if (ObjectMass > 0)
            {
                CurrentVelocity += impulse / ObjectMass;
            }
            else
            {
                CurrentVelocity += impulse;
            }
        }
        
        public void Stop()
        {
            CurrentVelocity = Vector2.zero;
            _currentAcceleration = Vector2.zero;
        }
        
        private void ApplyFriction()
        {
            float effectiveFriction = Friction;

            if (ObjectMass > 0)
            {
                effectiveFriction = Friction / (1 + ObjectMass);
            }

            if (CurrentVelocity.magnitude > effectiveFriction)
            {
                CurrentVelocity -= CurrentVelocity.normalized * effectiveFriction;
            }
            else
            {
                CurrentVelocity = Vector2.zero;
            }
        }
    }
}
