using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Projectiles
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolableObjectType _poolableObjectType;

        private Transform _transform;
        //pool reference.
        private float _speed = 0;

        private bool _delayedDestruction = false;
        private float _destroyAfter = 1f;

        public PoolableObjectType PoolKey => _poolableObjectType;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        public void Initialize(float speed, bool delayedDestruction = false, float destroyAfter = 1)
        {
            _speed = speed;
            _delayedDestruction = delayedDestruction;
            _destroyAfter = destroyAfter;
        }

        private void Update()
        {
            //move
        }

        public void OnSpawn()
        {
            gameObject.SetActive(true);
        }

        public void Despawn()
        {
            gameObject.SetActive(false);
        }
    }
}
