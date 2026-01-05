using UnityEngine;

namespace Core.Systems.ObjectPools
{
    [System.Serializable]
    public class PoolableObjectRegistryEntry
    {
        [field: SerializeField] public PoolableObjectType Type { get; private set; }
        [field: SerializeField] public PoolableObject Prefab { get; private set; }
        [field: SerializeField, Min(0)] public int InitialSize { get; private set; } = 20;
        [field: SerializeField, Min(1)] public int MaxSize { get; private set; } = 50;
    }
}
