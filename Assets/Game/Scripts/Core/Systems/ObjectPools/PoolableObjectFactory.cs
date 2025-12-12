using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Systems.ObjectPools
{
    public class PoolableObjectFactory
    {
        private readonly DiContainer _container;
        private readonly PoolableObjectRegistry _registry;

        [Inject]
        public PoolableObjectFactory(DiContainer container, PoolableObjectRegistry registry)
        {
            _container = container;
            _registry = registry;
        }

        public PoolableObject Create(PoolableObjectType type, Transform parent)
        {
            PoolableObject prefab = _registry.GetPrefab(type);

            if (prefab == null)
            {
                Debug.LogError($"Cannot create object of type {type}: prefab not found");
                return null;
            }

            PoolableObject instance = _container.InstantiatePrefabForComponent<PoolableObject>(prefab.gameObject, parent);

            return instance;
        }
    }
}
