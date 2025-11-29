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

        private Rigidbody2D _rigidbody2D;

        public bool ShouldTeleport => _shouldTeleport;
        public Rigidbody2D Rigidbody2D => _rigidbody2D;

        protected virtual void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void EnableTeleportation()
        {
            _shouldTeleport = true;
        }
    }
}
