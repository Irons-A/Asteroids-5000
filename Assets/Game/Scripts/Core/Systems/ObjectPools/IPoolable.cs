using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Systems.ObjectPools
{
    public interface IPoolable
    {
        PoolableObjectType PoolKey { get; }
        void OnSpawn();
        void Despawn();
    }
}
