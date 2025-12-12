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
    }
}
