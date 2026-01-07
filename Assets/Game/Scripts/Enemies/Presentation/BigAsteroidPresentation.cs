using Core.Components;
using Core.Configuration;
using Core.Configuration.Enemies;
using Core.Systems.ObjectPools;
using Enemies.Logic;
using Zenject;

namespace Enemies.Presentation
{
    public class BigAsteroidPresentation : EnemyPresentation
    {
        private BigAsteroidLogic _logic;
        private SpriteRotator _spriteRotator;
        private BigAsteroidSettings _settings;
        
        [Inject]
        private void Construct(BigAsteroidLogic logic, JsonConfigProvider jsonConfigProvider)
        {
            _logic = logic;
            _settings = jsonConfigProvider.BigAsteroidSettingsRef;
        }

        protected override void Awake()
        {
            base.Awake();
            
            PoolableObject = GetComponent<PoolableObject>();
            CollisionHandler = GetComponent<CollisionHandler>();
            
            _spriteRotator = GetComponentInChildren<SpriteRotator>();
            _spriteRotator.SetParameters(_settings.MinSpriteRotationSpeed, _settings.MaxSpriteRotationSpeed);
            
            _logic.Configure(this, PoolableObject, CollisionHandler);
        }
        
        private void OnEnable()
        {
            _logic.OnPresentationEnabled();
            SetAngle(0, false, true);
        }

        private void FixedUpdate()
        {
            _logic.Move();
        }
    }
}
