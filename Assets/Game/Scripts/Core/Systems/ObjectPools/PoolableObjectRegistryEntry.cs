using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [System.Serializable]
    public class PoolableObjectRegistryEntry
    {
        public PoolableObjectType type;
        public PoolableObject prefab;

        [SerializeField] private int _initialSize = 20;
        [SerializeField] private int _maxSize = 50;

        public int InitialSize => _initialSize;
        public int MaxSize => _maxSize;
    }
}
