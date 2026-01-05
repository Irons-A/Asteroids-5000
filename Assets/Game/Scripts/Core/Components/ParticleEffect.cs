using Core.Systems.ObjectPools;
using UnityEngine;

namespace Core.Components
{
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(PoolableObject))]
    public class ParticleEffect : MonoBehaviour
    {
        private const string TargetSortingLayerName = "Effects";
        
        private PoolableObject _poolableObject;
        private Renderer _renderer;

        private void Awake()
        {
            _poolableObject = GetComponent<PoolableObject>();
            _renderer = GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            _renderer.sortingLayerName = TargetSortingLayerName;
        }
        
        private void OnParticleSystemStopped()
        {
            _poolableObject.Despawn();
        }
    }
}