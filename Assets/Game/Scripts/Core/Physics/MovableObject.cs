using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
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
