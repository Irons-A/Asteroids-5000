using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovableObject : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;

        public Rigidbody2D Rigidbody2D => _rigidbody2D;

        private void Awake()
        {
            if (_rigidbody2D == null)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
            }
        }
    }
}
