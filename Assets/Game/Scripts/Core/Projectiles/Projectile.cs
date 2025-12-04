using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Projectiles
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private Transform _transform;
        private PoolableObject _poolableObject;

        private float _speed = 0;
        private bool _delayedDestruction = false;
        private float _destroyAfter = 1f;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            
            if (TryGetComponent(out PoolableObject poolableObject))
            {
                _poolableObject = poolableObject;
            }
        }

        public void Initialize(float speed, bool delayedDestruction = false, float destroyAfter = 1)
        {
            _speed = speed;
            _delayedDestruction = delayedDestruction;
            _destroyAfter = destroyAfter;
        }

        private void Update()
        {
            _transform.Translate(transform.right * _speed * Time.deltaTime);
        }


    }
}
