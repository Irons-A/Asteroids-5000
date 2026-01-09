using Core.Systems.ObjectPools;
using UnityEngine;

namespace Core.Systems
{
    public class ParticleService
    {
        private readonly PoolAccessProvider _poolAccessProvider;
        
        public ParticleService(PoolAccessProvider poolAccessProvider)
        {
            _poolAccessProvider = poolAccessProvider;
        }
        
        public void SpawnParticles(PoolableObjectType particleType, Vector3 position)
        {
            PoolableObject particle = _poolAccessProvider.GetFromPool(particleType);
            
            if (particle.TryGetComponent(out ParticleSystem _) == false)
            {
                particle.Despawn();
            }
            
            particle.transform.position = position;
        }
    }
}
