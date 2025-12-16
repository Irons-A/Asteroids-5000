using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [CreateAssetMenu(menuName = "ScriptableObjects/PoolableObjectRegistry", order = 51)]
    public class PoolableObjectRegistry : ScriptableObject
    {
        [SerializeField] private List<PoolableObjectRegistryEntry> _registry = new();

        private Dictionary<PoolableObjectType, PoolableObjectRegistryEntry> _cache;

        public PoolableObjectRegistryEntry GetEntry(PoolableObjectType type)
        {
            BuildCacheIfNeeded();

            if (_cache.TryGetValue(type, out var entry))
            {
                return entry;
            }

            Debug.LogError($"Entry for type {type} not found in registry");

            return null;
        }

        public PoolableObject GetPrefab(PoolableObjectType type)
        {
            PoolableObjectRegistryEntry entry = GetEntry(type);

            return entry.Prefab;
        }

        public int GetMaxSize(PoolableObjectType type)
        {
            PoolableObjectRegistryEntry entry = GetEntry(type);

            return entry.MaxSize;
        }

        public PoolableObjectType[] GetAllRegisteredTypes()
        {
            BuildCacheIfNeeded();

            PoolableObjectType[] types = new PoolableObjectType[_cache.Count];

            _cache.Keys.CopyTo(types, 0);

            return types;
        }

        private void BuildCacheIfNeeded()
        {
            if (_cache == null)
            {
                _cache = new Dictionary<PoolableObjectType, PoolableObjectRegistryEntry>();

                foreach (var entry in _registry)
                {
                    if (entry.Prefab != null)
                    {
                        _cache[entry.Type] = entry;
                    }
                }
            }
        }
    }
}
