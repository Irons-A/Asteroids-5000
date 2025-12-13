using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Systems.ObjectPools
{
    [RequireComponent(typeof(OutsideOfViewportDestroyer))]
    public class PoolableObject : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private PoolableObjectType _poolKey;
        [SerializeField] private DespawnCondition _despawnCondition;

        public PoolableObjectType PoolKey => _poolKey;
        public DespawnCondition DespawnCondition => _despawnCondition;

        private UniversalObjectPool _parentPool;
        private OutsideOfViewportDestroyer _viewportDestroyer;

        private void Awake()
        {
            _viewportDestroyer = GetComponent<OutsideOfViewportDestroyer>();
        }

        private void OnEnable()
        {
            if (_despawnCondition == DespawnCondition.OutsideOfViewport)
            {
                _viewportDestroyer.OnLeftViewport += Despawn;
            }
        }

        private void OnDisable()
        {
            _viewportDestroyer.OnLeftViewport -= Despawn;
        }

        private void Update()
        {
            if (_despawnCondition == DespawnCondition.OutsideOfViewport)
            {
                _viewportDestroyer.ProcessChecks();
            }
        }

        public void SetParentPool(UniversalObjectPool pool)
        {
            _parentPool = pool;
        }

        public void Despawn()
        {
            if (_parentPool != null)
            {
                _parentPool.ReturnToPool(this);
            }
            else
            {
                Debug.LogWarning($"PoolableObject {name} has no parent pool reference");

                Destroy(this);
            }
        }
    }
}
