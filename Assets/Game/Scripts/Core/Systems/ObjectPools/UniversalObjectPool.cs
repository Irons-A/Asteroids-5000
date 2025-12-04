using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    public class UniversalObjectPool : MonoBehaviour
    {
        [Header("Pool Configuration")]
        [SerializeField] private List<PoolConfig> _poolConfigs = new List<PoolConfig>();

        private Dictionary<PoolableObjectType, Queue<PoolableObject>> _pools = 
            new Dictionary<PoolableObjectType, Queue<PoolableObject>>();
        private Dictionary<PoolableObjectType, Transform> _poolContainers = new Dictionary<PoolableObjectType, Transform>();
        private Dictionary<PoolableObjectType, PoolConfig> _configs = new Dictionary<PoolableObjectType, PoolConfig>();

        private void Awake()
        {
            InitializeAllPools();
        }

        public PoolableObject GetFromPool(PoolableObjectType key)
        {
            if (!_pools.ContainsKey(key))
            {
                Debug.LogError($"Pool for {key} doesn't exist");

                return null;
            }

            PoolableObject objectToGet;

            if (_pools[key].Count > 0)
            {
                objectToGet = _pools[key].Dequeue();
            }
            else
            {
                int activeCount = CountActiveObjects(key);

                if (activeCount >= _configs[key].MaxSize)
                {
                    Debug.LogWarning($"Pool {key} reached max size ({_configs[key].MaxSize})");

                    return null;
                }

                objectToGet = CreateNewObject(key);

                if (objectToGet == null) return null;
            }

            objectToGet.gameObject.SetActive(true);

            return objectToGet;
        }

        public void ReturnToPool(PoolableObject poolableObject)
        {
            if (poolableObject == null) return;

            PoolableObjectType key = poolableObject.PoolKey;

            if (!_pools.ContainsKey(key))
            {
                Debug.LogError($"Trying to return object with unknown key: {key}");

                Destroy(poolableObject.gameObject);

                return;
            }

            poolableObject.gameObject.SetActive(false);
            poolableObject.transform.SetParent(_poolContainers[key]);

            _pools[key].Enqueue(poolableObject);
        }

        public bool HasPoolFor(PoolableObjectType key)
        {
            return _pools.ContainsKey(key);
        }

        private void InitializeAllPools()
        {
            foreach (var config in _poolConfigs)
            {
                InitializePool(config);
            }
        }

        private void InitializePool(PoolConfig config)
        {
            if (config.Prefab == null)
            {
                Debug.LogWarning($"Prefab for {config.Key} is null");
                return;
            }

            GameObject container = new GameObject($"Pool_{config.Key}");

            container.transform.SetParent(transform);
            _poolContainers[config.Key] = container.transform;

            _pools[config.Key] = new Queue<PoolableObject>();

            _configs[config.Key] = config;

            WarmUpPool(config.Key, config.InitialSize);
        }

        private void WarmUpPool(PoolableObjectType key, int count)
        {
            if (!_configs.ContainsKey(key)) return;

            for (int i = 0; i < count; i++)
            {
                PoolableObject poolableObject = CreateNewObject(key);

                ReturnToPool(poolableObject);
            }
        }

        private PoolableObject CreateNewObject(PoolableObjectType key)
        {
            if (!_configs.ContainsKey(key))
            {
                Debug.LogError($"Pool config not found for: {key}");

                return null;
            }

            PoolableObject prefab = _configs[key].Prefab;
            Transform container = _poolContainers[key];
            PoolableObject newObject = Instantiate(prefab, container);

            newObject.Initialize(this);

            return newObject;
        }

        private int CountActiveObjects(PoolableObjectType key)
        {
            if (!_poolContainers.ContainsKey(key)) return 0;

            int activeCount = 0;

            foreach (Transform child in _poolContainers[key])
            {
                if (child.gameObject.activeSelf) activeCount++;
            }

            return activeCount;
        }
    }
}
