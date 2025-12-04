using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [System.Serializable]
    public class PoolConfig
    {
        public PoolableObjectType Key;
        public PoolableObject Prefab; 
        public int InitialSize = 20;
        public int MaxSize = 50;
    }
}
