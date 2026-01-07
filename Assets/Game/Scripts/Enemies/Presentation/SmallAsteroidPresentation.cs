using Core.Components;
using Core.Configuration;
using Core.Configuration.Enemies;
using Core.Systems.ObjectPools;
using Enemies.Logic;
using Zenject;

namespace Enemies.Presentation
{
    public class SmallAsteroidPresentation : EnemyPresentation
    {
        private SmallAsteroidLogic _logic;
        private SpriteRotator _spriteRotator;
        private SmallAsteroidSettings _settings;
        
        [Inject]
        private void Construct(SmallAsteroidLogic logic, JsonConfigProvider jsonConfigProvider)
        {
            _logic = logic;
            _settings = jsonConfigProvider.SmallAsteroidSettingsRef;
        }

        protected override void Awake()
        {
            _shouldTeleport = true;
            
            PoolableObject = GetComponent<PoolableObject>();
            CollisionHandler = GetComponent<CollisionHandler>();
            
            _spriteRotator = GetComponentInChildren<SpriteRotator>();
            _spriteRotator.SetParameters(_settings.MinSpriteRotationSpeed, _settings.MaxSpriteRotationSpeed);
            
            _logic.Configure(this, PoolableObject, CollisionHandler);
        }
        
        private void OnEnable()
        {
            _logic.OnPresentationEnabled();
        }

        private void FixedUpdate()
        {
            _logic.Move();
        }
    }
}
