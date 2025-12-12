using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

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

        public PoolableObject GetFromPool(PoolableObjectType key)
        {
            if (!_pools.ContainsKey(key))
            {
                Debug.LogError($"Pool for {key} doesn't exist");
                return null;
            }

            PoolableObject obj;

            if (_pools[key].Count > 0)
            {
                obj = _pools[key].Dequeue();
            }
            else
            {
                // Проверяем лимит перед созданием нового
                int activeCount = CountActiveObjects(key);
                if (activeCount >= _configs[key].MaxSize)
                {
                    Debug.LogWarning($"Pool {key} reached max size ({_configs[key].MaxSize})");
                    return null;
                }

                // Создаем новый объект через фабрику
                PoolableObject newObj = CreateNewObject(key);
                if (newObj == null) return null;
                obj = newObj;
            }

            obj.gameObject.SetActive(true);
            return obj;
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
            if (_registry.GetPrefab(config.Key) == null)
            {
                Debug.LogError($"Prefab for {config.Key} not found in registry");
                return;
            }

            GameObject container = new GameObject($"Pool_{config.Key}");
            container.transform.SetParent(transform);
            _poolContainers[config.Key] = container.transform;

            // Инициализируем очередь
            _pools[config.Key] = new Queue<PoolableObject>();
            _configs[config.Key] = config;

            // Наполняем пул через фабрику
            WarmUpPool(config.Key, config.InitialSize);
        }

        private void WarmUpPool(PoolableObjectType key, int count)
        {
            if (!_configs.ContainsKey(key)) return;

            for (int i = 0; i < count; i++)
            {
                // Создаем объект через фабрику
                PoolableObject obj = CreateNewObject(key);
                if (obj != null)
                {
                    ReturnToPool(obj);
                }
            }
        }

        private PoolableObject CreateNewObject(PoolableObjectType key)
        {
            PoolableObject obj = _factory.Create(key, _poolContainers[key]);

            obj.SetParentPool(this);

            return obj;
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
