using Core.Systems.ObjectPools;
using Core.Logic;
using Core.Systems;
using UnityEngine;
using Zenject;

namespace Core.Components
{
    [RequireComponent(typeof(CollisionHandler))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private bool _shouldSpawnHitParticles = true;
        
        private PoolableObject _poolableObject;
        private ProjectileLogic _logic;
        private CollisionHandler _collisionHandler;
        private ParticleService _particleService;

        [Inject]
        private void Construct(ProjectileLogic logic, ParticleService particleService)
        {
            _logic = logic;
            _logic.SetPresentationTransform(transform);
            _logic.OnDelayedDestructionCalled += CallDespawn;
            
            _particleService = particleService;
        }
        
        private void Awake()
        {
            _collisionHandler = GetComponent<CollisionHandler>();
            _collisionHandler.OnDestructionCalled += CallDespawn;
            
            if (TryGetComponent(out PoolableObject poolableObject))
            {
                _poolableObject = poolableObject;
            }
        }

        private void Update()
        {
            _logic.MoveProjectile();
        }

        public void Configure(float speed, bool delayedDestruction = false, float destroyAfter = 1)
        {
            _logic.ConfigureParameters(speed, delayedDestruction, destroyAfter);
        }

        private void CallDespawn()
        {
            _logic.CancelDelayedDestruction();

            if (_shouldSpawnHitParticles)
            {
                _particleService.SpawnParticles(PoolableObjectType.ProjectileHitParticles, transform.position);
            }
            
            if (_poolableObject != null)
            {
                _poolableObject.Despawn();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            _logic.OnDelayedDestructionCalled -= CallDespawn;
            _collisionHandler.OnDestructionCalled -= CallDespawn;
            _logic.CancelDelayedDestruction();
        }
    }
}
