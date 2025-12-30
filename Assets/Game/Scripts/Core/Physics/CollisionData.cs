using System.Collections;
using System.Collections.Generic;
using Core.Components;
using UnityEngine;

namespace Core.Physics
{
    public struct CollisionData
    {
        public Vector2 point;
        public Vector2 normal;
        public float otherRestitution;
        public float otherFriction;
        public float otherMass;
        public Vector2 otherVelocity;
    
        public CollisionData(Vector2 point, Vector2 normal, float otherRestitution = 0.8f, 
            float otherFriction = 0.1f, float otherMass = 1f, Vector2 otherVelocity = default)
        {
            this.point = point;
            this.normal = normal;
            this.otherRestitution = otherRestitution;
            this.otherFriction = otherFriction;
            this.otherMass = otherMass;
            this.otherVelocity = otherVelocity;
        }
    }
}
