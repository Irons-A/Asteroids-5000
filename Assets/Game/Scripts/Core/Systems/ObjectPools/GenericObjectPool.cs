using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    public class GenericObjectPool : MonoBehaviour
    {
        [Header("Pool Configuration")]
        [SerializeField] protected List<PoolConfig> _poolConfigs = new List<PoolConfig>();

        protected Dictionary<PoolableObjectType, Queue<GameObject>> _pools = 
            new Dictionary<PoolableObjectType, Queue<GameObject>>();
        protected Dictionary<PoolableObjectType, Transform> _poolContainers = 
            new Dictionary<PoolableObjectType, Transform>();
        protected Dictionary<PoolableObjectType, PoolConfig> _configs = 
            new Dictionary<PoolableObjectType, PoolConfig>();

        protected virtual void Awake()
        {
            InitializeAllPools();
        }

        protected virtual void InitializeAllPools()
        {
            foreach (var config in _poolConfigs)
            {
                InitializePool(config);
            }
        }

        protected virtual void InitializePool(PoolConfig config)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning($"Prefab for {config.key} is null");
                return;
            }

            // Проверяем, что префаб реализует IPoolable
            if (config.prefab is IPoolable == false)
            {
                Debug.LogError($"Prefab for {config.key} doesn't implement IPoolable interface");
                return;
            }

            // Создаем контейнер
            GameObject container = new GameObject($"Pool_{config.key}");
            container.transform.SetParent(transform);
            _poolContainers[config.key] = container.transform;

            // Инициализируем очередь
            _pools[config.key] = new Queue<GameObject>();
            _configs[config.key] = config;

            // Наполняем пул
            WarmUpPool(config.key, config.initialSize);
        }

        protected virtual void WarmUpPool(PoolableObjectType key, int count)
        {
            if (!_configs.ContainsKey(key)) return;

            for (int i = 0; i < count; i++)
            {
                GameObject obj = CreateNewObject(key);
                ReturnToPool(obj);
            }
        }

        protected virtual GameObject CreateNewObject(PoolableObjectType key)
        {
            if (!_configs.ContainsKey(key))
            {
                Debug.LogError($"Pool config not found for: {key}");
                return null;
            }

            GameObject prefab = _configs[key].prefab;
            Transform container = _poolContainers[key];

            GameObject newObj = Instantiate(prefab, container);

            // Получаем IPoolable компонент и вызываем Despawn
            if (newObj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.Despawn();
            }
            else
            {
                Debug.LogError($"Instantiated object doesn't implement IPoolable");
                Destroy(newObj);
                return null;
            }

            return newObj;
        }

        // Основной метод получения объекта
        public virtual GameObject GetFromPool(PoolableObjectType key, Vector3 position, Quaternion rotation)
        {
            if (!_pools.ContainsKey(key))
            {
                Debug.LogError($"Pool for {key} doesn't exist");
                return null;
            }

            GameObject obj;

            if (_pools[key].Count > 0)
            {
                obj = _pools[key].Dequeue();
            }
            else
            {
                // Проверяем лимит перед созданием нового
                int activeCount = CountActiveObjects(key);
                if (activeCount >= _configs[key].maxSize)
                {
                    Debug.LogWarning($"Pool {key} reached max size ({_configs[key].maxSize})");
                    return null;
                }

                obj = CreateNewObject(key);
                if (obj == null) return null;
            }

            // Устанавливаем позицию, поворот
            obj.transform.SetPositionAndRotation(position, rotation);

            // Вызываем OnSpawn через интерфейс
            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnSpawn();
            }

            return obj;
        }

        // Упрощенные методы
        public virtual GameObject GetFromPool(PoolableObjectType key)
        {
            return GetFromPool(key, Vector3.zero, Quaternion.identity);
        }

        public virtual GameObject GetFromPool(PoolableObjectType key, Vector3 position)
        {
            return GetFromPool(key, position, Quaternion.identity);
        }

        // Возврат объекта в пул
        public virtual void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            // Получаем тип через интерфейс
            if (!obj.TryGetComponent<IPoolable>(out var poolable))
            {
                Debug.LogError($"Object doesn't implement IPoolable interface");
                Destroy(obj);
                return;
            }

            PoolableObjectType key = poolable.PoolKey;

            if (!_pools.ContainsKey(key))
            {
                Debug.LogError($"Trying to return object with unknown key: {key}");
                Destroy(obj);
                return;
            }

            // Вызываем Despawn и возвращаем в контейнер
            poolable.Despawn();
            obj.transform.SetParent(_poolContainers[key]);
            _pools[key].Enqueue(obj);
        }

        // Вспомогательные методы
        public virtual int CountActiveObjects(PoolableObjectType key)
        {
            if (!_poolContainers.ContainsKey(key)) return 0;

            int activeCount = 0;
            foreach (Transform child in _poolContainers[key])
            {
                if (child.gameObject.activeSelf) activeCount++;
            }
            return activeCount;
        }

        public virtual int GetPoolSize(PoolableObjectType key)
        {
            return _pools.ContainsKey(key) ? _pools[key].Count : 0;
        }

        public virtual bool HasPoolFor(PoolableObjectType key)
        {
            return _pools.ContainsKey(key);
        }

        // Получение компонента по типу
        public virtual T GetFromPool<T>(PoolableObjectType key, Vector3 position, Quaternion rotation) where T : class, IPoolable
        {
            GameObject obj = GetFromPool(key, position, rotation);
            if (obj != null && obj.TryGetComponent<T>(out var component))
            {
                return component;
            }
            return null;
        }

        // Метод для динамического добавления конфигураций
        public virtual bool AddPoolConfig(PoolConfig config)
        {
            if (_pools.ContainsKey(config.key))
            {
                Debug.LogWarning($"Pool for {config.key} already exists");
                return false;
            }

            _poolConfigs.Add(config);
            InitializePool(config);
            return true;
        }
    }
}
