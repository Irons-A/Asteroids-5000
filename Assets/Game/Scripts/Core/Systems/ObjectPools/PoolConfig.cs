using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [System.Serializable]
    public class PoolConfig
    {
        public string key;
        public IPoolable prefab;
        public int initialSize = 20;
        public int maxSize = 50;
    }
}
