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
        private const float RicochetMagnitudeThreshold = 0.1f;
        
        private Transform _movableObjectTransform;
        private Vector2 _currentAcceleration;
        
        public float ObjectMass { get; private set; }
        public float Friction { get; private set; }
        public float Restitution { get; private set; }
        public Vector2 CurrentVelocity { get; private set; }
        public float CurrentSpeed { get; private set; }

        public void SetMovableObject(MovableObject movableObject, float objectMass = BaseObjectMass, 
            float friction = BaseFriction, float restitution = BaseRestitution)
        {
            _movableObjectTransform = movableObject.transform;
            ObjectMass = objectMass <= 0 ? BaseObjectMass : objectMass;
            Friction = friction <= 0 ? BaseFriction : friction;
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
            Vector2 normal = collisionData.Normal.normalized;
            Vector2 velocity = CurrentVelocity;
            Vector2 otherVelocity = collisionData.OtherVelocity;
            
            Vector2 relativeVelocity = velocity - otherVelocity;
            
            float normalRelativeSpeed = Vector2.Dot(relativeVelocity, normal);
            
            if (normalRelativeSpeed > 0)
            {
                return;
            }
            
            float restitution1 = Restitution;
            float restitution2 = collisionData.OtherRestitution;
            float effectiveRestitution = Mathf.Min(restitution1, restitution2);
            
            float friction1 = Friction;
            float friction2 = collisionData.OtherFriction;
            float effectiveFriction = (friction1 + friction2) * 0.5f;
            
            float thisMass = ObjectMass;
            float otherMass = collisionData.OtherMass;
            float totalMass = thisMass + otherMass;
            
            float impulseMagnitude = -(1 + effectiveRestitution) * normalRelativeSpeed;
            impulseMagnitude *= (thisMass * otherMass) / totalMass;
            
            Vector2 impulse = normal * impulseMagnitude;
            
            Vector2 newVelocity = velocity + impulse / thisMass;
            
            if (newVelocity.magnitude > RicochetMagnitudeThreshold)
            {
                float normalSpeed = Vector2.Dot(newVelocity, normal);
                Vector2 normalVel = normal * normalSpeed;
                Vector2 tangentVel = newVelocity - normalVel;
                
                tangentVel *= (1f - effectiveFriction);
                newVelocity = normalVel + tangentVel;
            }
            
            CurrentVelocity = newVelocity;
            
            if (newVelocity.magnitude > RicochetMagnitudeThreshold)
            {
                float angle = Mathf.Atan2(newVelocity.y, newVelocity.x) * Mathf.Rad2Deg;
                _movableObjectTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public void SetInstantVelocity(float speed)
        {
            if (_movableObjectTransform == null) return;
            
            Vector2 direction = _movableObjectTransform.right;
            
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
