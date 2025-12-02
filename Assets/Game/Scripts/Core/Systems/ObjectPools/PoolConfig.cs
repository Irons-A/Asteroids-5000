using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [System.Serializable]
    public class PoolConfig
    {
        public PoolableObjectType key;
        public GameObject prefab;
        public int initialSize = 20;
        public int maxSize = 50;
    }
}
