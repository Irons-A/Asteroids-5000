using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [CreateAssetMenu(menuName = "ScriptableObjects/PoolableObjectRegistry", order = 51)]
    public class PoolableObjectRegistry : ScriptableObject
    {
        [SerializeField] private List<PoolableObjectRegistryEntry> _registry = new List<PoolableObjectRegistryEntry>();

        private Dictionary<PoolableObjectType, PoolableObject> _cache;

        public PoolableObject GetPrefab(PoolableObjectType type)
        {
            if (_cache == null)
            {
                GenerateCashe();
            }

            if (_cache.TryGetValue(type, out var prefab))
            {
                return prefab;
            }

            Debug.LogError($"Prefab for type {type} not found in registry");

            return null;
        }

        private void GenerateCashe()
        {
            _cache = new Dictionary<PoolableObjectType, PoolableObject>();

            foreach (var entry in _registry)
            {
                if (entry.prefab != null)
                {
                    _cache[entry.type] = entry.prefab;
                }
            }
        }
    }
}
