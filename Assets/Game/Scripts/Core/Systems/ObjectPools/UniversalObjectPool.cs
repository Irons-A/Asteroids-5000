using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Core.Systems.ObjectPools
{
    public class UniversalObjectPool : MonoBehaviour
    {
        private Dictionary<PoolableObjectType, Queue<PoolableObject>> _pools = 
            new Dictionary<PoolableObjectType, Queue<PoolableObject>>();
        private Dictionary<PoolableObjectType, Transform> _poolContainers = 
            new Dictionary<PoolableObjectType, Transform>();

        private PoolableObjectFactory _factory;
        private PoolableObjectRegistry _registry;

        [Inject]
        public void Construct(PoolableObjectFactory factory, PoolableObjectRegistry registry)
        {
            _factory = factory;
            _registry = registry;
        }

        private void Awake()
        {
            InitializeAllPools();
        }

        public PoolableObject GetFromPool(PoolableObjectType type)
        {
            if (_pools.ContainsKey(type) == false)
            {
                Debug.LogError($"Pool for {type} doesn't exist");

                return null;
            }

            PoolableObject poolableObject;

            if (_pools[type].Count > 0)
            {
                poolableObject = _pools[type].Dequeue();
            }
            else
            {
                int activeCount = CountActiveObjects(type);
                int maxSize = _registry.GetMaxSize(type);

                if (activeCount >= maxSize)
                {
                    Debug.LogWarning($"Pool {type} reached max size ({maxSize})");

                    return null;
                }

                PoolableObject newObject = CreateNewObject(type);

                if (newObject == null) return null;

                poolableObject = newObject;
            }

            poolableObject.gameObject.SetActive(true);

            return poolableObject;
        }

        public void ReturnToPool(PoolableObject poolableObject)
        {
            if (poolableObject == null) return;

            PoolableObjectType type = poolableObject.PoolKey;

            if (_pools.ContainsKey(type) == false)
            {
                Debug.LogError($"Trying to return object with unknown type: {type}");

                return;
            }

            poolableObject.gameObject.SetActive(false);
            poolableObject.transform.SetParent(_poolContainers[type]);

            _pools[type].Enqueue(poolableObject);
        }

        private void InitializeAllPools()
        {
            PoolableObjectType[] allTypes = _registry.GetAllRegisteredTypes();

            foreach (PoolableObjectType type in allTypes)
            {
                InitializePool(type);
            }
        }

        private void InitializePool(PoolableObjectType type)
        {
            PoolableObjectRegistryEntry entry = _registry.GetEntry(type);

            if (entry == null || entry.Prefab == null)
            {
                Debug.LogError($"Cannot initialize pool for {type}: entry not found");

                return;
            }

            GameObject container = new GameObject($"Pool_{type}");

            container.transform.SetParent(transform);
            _poolContainers[type] = container.transform;

            _pools[type] = new Queue<PoolableObject>();

            WarmUpPool(type, entry.InitialSize);
        }

        private void WarmUpPool(PoolableObjectType type, int count)
        {
            if (_poolContainers.ContainsKey(type) == false) return;

            for (int i = 0; i < count; i++)
            {
                PoolableObject poolableObject = CreateNewObject(type);

                if (poolableObject != null)
                {
                    ReturnToPool(poolableObject);
                }
            }
        }

        private PoolableObject CreateNewObject(PoolableObjectType key)
        {
            PoolableObject poolableObject = _factory.Create(key, _poolContainers[key]);

            poolableObject.SetParentPool(this);

            return poolableObject;
        }

        private int CountActiveObjects(PoolableObjectType key)
        {
            if (_poolContainers.ContainsKey(key) == false) return 0;

            int activeCount = 0;

            foreach (Transform child in _poolContainers[key])
            {
                if (child.gameObject.activeSelf) activeCount++;
            }

            return activeCount;
        }
    }
}
