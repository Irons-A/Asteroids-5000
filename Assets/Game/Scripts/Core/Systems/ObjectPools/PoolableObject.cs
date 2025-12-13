using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Systems.ObjectPools
{
    public class PoolableObject : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private PoolableObjectType _poolKey;
        [SerializeField] private DespawnCondition _despawnCondition;

        [Header("Outside of viewport destruction")]
        [SerializeField] private float _viewportMargin = 0.2f;
        [SerializeField] private float _checkInterval = 0.1f;

        public PoolableObjectType PoolKey => _poolKey;
        public DespawnCondition DespawnCondition => _despawnCondition;

        private UniversalObjectPool _parentPool;
        private ViewportDestroyer _viewportDestroyer;

        [Inject]
        public void Construct(ViewportDestroyer viewportDestroyer)
        {
            _viewportDestroyer = viewportDestroyer;
            _viewportDestroyer.Configure(transform, _viewportMargin, _checkInterval);
        }

        private void OnEnable()
        {
            if (_despawnCondition == DespawnCondition.OutsideOfViewport)
            {
                _viewportDestroyer.SetIsActive(true);
                _viewportDestroyer.OnLeftViewport += Despawn;
            }
        }

        private void OnDisable()
        {
            _viewportDestroyer.SetIsActive(false);
            _viewportDestroyer.OnLeftViewport -= Despawn;
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
