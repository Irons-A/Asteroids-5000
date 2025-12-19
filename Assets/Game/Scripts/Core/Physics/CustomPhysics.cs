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

        private MovableObject _movableObject;

        private Vector2 _currentAcceleration;
        private Vector2 _currentVelocity;

        // Для отслеживания последнего движения (опционально)
        private Vector2 _lastFramePosition;
        private float _lastMoveTime;

        public void SetMovableObject(MovableObject movableObject, float friction = BaseFriction,
            float objectMass = BaseObjectMass)
        {
            _movableObject = movableObject;
            _friction = friction <= 0 ? BaseFriction : friction;
            _objectMass = objectMass <= 0 ? BaseObjectMass : objectMass;

            _currentAcceleration = Vector2.zero;
            _currentVelocity = Vector2.zero;
            
            if (_movableObject != null)
            {
                _lastFramePosition = _movableObject.transform.position;
            }
        }

        public void ApplyAcceleration(float acceleration, float maxSpeed)
        {
            if (_movableObject == null) return;

            Vector2 direction = _movableObject.transform.right;

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
            if (_movableObject == null) return;

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
            if (_movableObject == null || _movableObject.transform == null) return;
            
            _currentVelocity += _currentAcceleration * Time.fixedDeltaTime;
            _currentAcceleration = Vector2.zero;
            
            if (_friction > 0)
            {
                ApplyFriction();
            }
            
            ApplyTransformMovement();
        }

        private void ApplyTransformMovement()
        {
            Vector2 movement = _currentVelocity * Time.fixedDeltaTime;
            
            _movableObject.transform.position += new Vector3(movement.x, movement.y, 0);
            
            _lastFramePosition = _movableObject.transform.position;
            _lastMoveTime = Time.time;
        }

        public void SetInstantVelocity(float speed)
        {
            if (_movableObject == null) return;

            Vector2 direction = _movableObject.transform.right;
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
        
        public Vector2 GetVelocity() => _currentVelocity;
        public float GetSpeed() => _currentVelocity.magnitude;
        public Vector2 GetLastFramePosition() => _lastFramePosition;
        
        public void ApplyExternalForce(Vector2 force)
        {
            float effectiveForce = force.magnitude;
            
            if (_objectMass > 0)
            {
                effectiveForce = force.magnitude / _objectMass;
            }
            
            _currentVelocity += force.normalized * effectiveForce;
        }
        
        public void Stop()
        {
            _currentVelocity = Vector2.zero;
            _currentAcceleration = Vector2.zero;
        }
        
        public void SmoothStop(float stopTime)
        {
            if (stopTime <= 0)
            {
                Stop();
                return;
            }
            
            float deceleration = _currentVelocity.magnitude / stopTime;
            ApplyDeceleration(deceleration);
        }
    }
}
