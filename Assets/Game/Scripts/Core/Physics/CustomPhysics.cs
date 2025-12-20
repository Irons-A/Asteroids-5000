using Codice.Client.Common.GameUI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Core.Physics
{
    public class CustomPhysics
    {
        public const float BaseFriction = 0;
        public const float BaseObjectMass = 0;

        private float _friction;
        private float _objectMass;

        private Transform _movableObjectTransform;

        private Vector2 _currentAcceleration;
        private Vector2 _currentVelocity;

        public void SetMovableObject(MovableObject movableObject, float friction = BaseFriction,
            float objectMass = BaseObjectMass)
        {
            _movableObjectTransform = movableObject.transform;
            _friction = friction <= 0 ? BaseFriction : friction;
            _objectMass = objectMass <= 0 ? BaseObjectMass : objectMass;

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
            
            if (_friction > 0)
            {
                ApplyFriction();
            }
            
            Vector2 movement = _currentVelocity * Time.fixedDeltaTime;
            
            _movableObjectTransform.position += new Vector3(movement.x, movement.y, 0);
        }

        public void ApplyRicochet()
        {
            _currentVelocity = -_currentVelocity;
        }

        public void SetInstantVelocity(float speed)
        {
            if (_movableObjectTransform == null) return;

            Vector2 direction = _movableObjectTransform.right;
            
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
