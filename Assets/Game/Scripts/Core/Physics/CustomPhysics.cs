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

        private IMovable _movableObject;

        private float _friction;
        private float _objectMass;

        public void SetMovableObject(IMovable movableObject, float friction = BaseFriction,
            float objectMass = BaseObjectMass)
        {
            _movableObject = movableObject;
            _friction = friction;
            _objectMass = objectMass;
        }

        public void ApplyAcceleration()
        {
            if (_movableObject == null) return;


        }

        public void ApplyDeceleration()
        {
            if (_movableObject == null) return;


        }

        public void UpdatePosition()
        {
            if (_movableObject == null) return;


        }

        public void SetInstantVelocity()
        {
            if (_movableObject == null) return;


        }
    }
}
