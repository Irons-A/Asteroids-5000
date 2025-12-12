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
            if (!_pools.ContainsKey(type))
            {
                Debug.LogError($"Pool for {type} doesn't exist");
                return null;
            }

            PoolableObject obj;

            if (_pools[type].Count > 0)
            {
                obj = _pools[type].Dequeue();
            }
            else
            {
                // Проверяем лимит перед созданием нового
                int activeCount = CountActiveObjects(type);
                int maxSize = _registry.GetMaxSize(type);

                if (activeCount >= maxSize)
                {
                    Debug.LogWarning($"Pool {type} reached max size ({maxSize})");
                    return null;
                }

                // Создаем новый объект через фабрику
                PoolableObject newObj = CreateNewObject(type);
                if (newObj == null) return null;
                obj = newObj;
            }

            obj.gameObject.SetActive(true);

            return obj;
        }

        public void ReturnToPool(PoolableObject poolableObject)
        {
            if (poolableObject == null) return;

            PoolableObjectType type = poolableObject.PoolKey;

            if (!_pools.ContainsKey(type))
            {
                Debug.LogError($"Trying to return object with unknown type: {type}");
                return;
            }

            PoolableObject obj = poolableObject;

            // Деактивируем и возвращаем в контейнер
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_poolContainers[type]);
            _pools[type].Enqueue(obj);
        }

        public bool HasPoolFor(PoolableObjectType key)
        {
            return _pools.ContainsKey(key);
        }

        private void InitializeAllPools()
        {
            var allTypes = _registry.GetAllRegisteredTypes();

            foreach (var type in allTypes)
            {
                InitializePool(type);
            }
        }

        private void InitializePool(PoolableObjectType type)
        {
            var entry = _registry.GetEntry(type);
            if (entry == null || entry.prefab == null)
            {
                Debug.LogError($"Cannot initialize pool for {type}: entry not found");
                return;
            }

            // Создаем контейнер
            GameObject container = new GameObject($"Pool_{type}");
            container.transform.SetParent(transform);
            _poolContainers[type] = container.transform;

            // Инициализируем очередь
            _pools[type] = new Queue<PoolableObject>();

            // Наполняем пул
            WarmUpPool(type, entry.InitialSize);
        }

        private void WarmUpPool(PoolableObjectType type, int count)
        {
            if (!_poolContainers.ContainsKey(type)) return;

            for (int i = 0; i < count; i++)
            {
                PoolableObject obj = CreateNewObject(type);
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
