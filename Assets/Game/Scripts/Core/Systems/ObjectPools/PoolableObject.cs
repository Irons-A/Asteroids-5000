using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    public class PoolableObject : MonoBehaviour
    {
        [SerializeField] private PoolableObjectType _poolKey;
        [SerializeField] private DespawnCondition _despawnCondition;

        public PoolableObjectType PoolKey => _poolKey;
        public DespawnCondition DespawnCondition => _despawnCondition;

        private GenericObjectPool _parentPool;

        public void Initialize(GenericObjectPool pool)
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
                gameObject.SetActive(false);
            }
        }
    }
}
