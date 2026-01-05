using Core.Components;
using UnityEngine;

namespace Core.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CollisionHandler))]
    [RequireComponent(typeof(Collider2D))]
    public class MovableObject : MonoBehaviour
    {
        // false if the object is meant to ignore first attempt to teleport it
        protected bool _shouldTeleport = false;
        
        public bool ShouldTeleport => _shouldTeleport;
        
        public void EnableTeleportation()
        {
            _shouldTeleport = true;
        }
    }
}
