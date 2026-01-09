using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [System.Serializable]
    public class PoolableObjectRegistryEntry
    {
        [SerializeField] private PoolableObjectType _type;
        [SerializeField] private PoolableObject _prefab;
        [SerializeField, Min(0)] private int _initialSize = 10;
        [SerializeField, Min(1)] private int _maxSize = 50;
        
        public PoolableObjectType Type => _type;
        public PoolableObject Prefab => _prefab;
        public int InitialSize => _initialSize;
        public int MaxSize => _maxSize;
    }
}
