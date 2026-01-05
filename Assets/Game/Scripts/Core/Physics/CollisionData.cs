using UnityEngine;

namespace Core.Physics
{
    public struct CollisionData
    {
        public readonly Vector2 Normal;
        public readonly float OtherRestitution;
        public readonly float OtherFriction;
        public readonly float OtherMass;
        public readonly Vector2 OtherVelocity;
    
        public CollisionData(Vector2 normal, float otherRestitution = 0.7f, 
            float otherFriction = 0, float otherMass = 1f, Vector2 otherVelocity = default)
        {
            Normal = normal;
            OtherRestitution = otherRestitution;
            OtherFriction = otherFriction;
            OtherMass = otherMass;
            OtherVelocity = otherVelocity;
        }
    }
}
