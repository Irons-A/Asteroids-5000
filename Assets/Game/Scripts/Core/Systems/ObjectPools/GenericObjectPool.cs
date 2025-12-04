using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    public class GenericObjectPool : MonoBehaviour
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

            // Создаем контейнер
            GameObject container = new GameObject($"Pool_{config.Key}");
            container.transform.SetParent(transform);
            _poolContainers[config.Key] = container.transform;

            // Инициализируем очередь
            _pools[config.Key] = new Queue<PoolableObject>();
            _configs[config.Key] = config;

            // Наполняем пул
            WarmUpPool(config.Key, config.InitialSize);
        }

        private void WarmUpPool(PoolableObjectType key, int count)
        {
            if (!_configs.ContainsKey(key)) return;

            for (int i = 0; i < count; i++)
            {
                PoolableObject obj = CreateNewObject(key);
                ReturnToPool(obj);
                //ReturnToPool(obj.GetComponent<PoolableObject>());
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

            PoolableObject newObj = Instantiate(prefab, container);

            // Инициализируем PoolableObject ссылкой на этот пул
            PoolableObject poolable = newObj.GetComponent<PoolableObject>();
            if (poolable == null)
            {
                Debug.LogError($"Instantiated object doesn't have PoolableObject component");
                Destroy(newObj);
                return null;
            }

            poolable.Initialize(this);
            return newObj;
        }

        // Основной метод получения объекта
        public PoolableObject GetFromPool(PoolableObjectType key, Vector3 position, Quaternion rotation)
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

                obj = CreateNewObject(key);

                if (obj == null) return null;
            }

            // Устанавливаем позицию, поворот и активируем
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);

            return obj;
        }

        // Возврат объекта в пул (вызывается из PoolableObject.Despawn())
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

            //GameObject obj = poolableObject.gameObject;

            // Деактивируем и возвращаем в контейнер
            poolableObject.gameObject.SetActive(false);
            poolableObject.transform.SetParent(_poolContainers[key]);
            _pools[key].Enqueue(poolableObject);
        }

        // Альтернативный метод возврата по GameObject REMOVE
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            PoolableObject poolable = obj.GetComponent<PoolableObject>();
            if (poolable != null)
            {
                ReturnToPool(poolable);
            }
            else
            {
                Debug.LogError($"GameObject {obj.name} doesn't have PoolableObject component");
                Destroy(obj);
            }
        }

        // Вспомогательные методы REMOVE
        public int CountActiveObjects(PoolableObjectType key)
        {
            if (!_poolContainers.ContainsKey(key)) return 0;

            int activeCount = 0;
            foreach (Transform child in _poolContainers[key])
            {
                if (child.gameObject.activeSelf) activeCount++;
            }
            return activeCount;
        }

        public bool HasPoolFor(PoolableObjectType key)
        {
            return _pools.ContainsKey(key);
        }
    }
}
