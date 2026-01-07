using Core.Components;
using Core.Configuration;
using Core.Configuration.Enemies;
using Core.Systems.ObjectPools;
using Enemies.Logic;
using UnityEngine;
using Zenject;

namespace Enemies.Presentation
{
    public class UFOPresentation : EnemyPresentation
    {
        private UFOLogic _logic;
        private SpriteRotator _spriteRotator;
        private UFOSettings _settings;
        
        [field: SerializeField] public Transform[] Firepoints { get; private set; }
        
        [Inject]
        private void Construct(UFOLogic logic, JsonConfigProvider jsonConfigProvider)
        {
            _logic = logic;
            _settings = jsonConfigProvider.UFOSettingsRef;
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
        }

        private void Update()
        {
            _logic.RotateTowardsPlayer();
        }

        private void FixedUpdate()
        {
            _logic.Move();
        }
    }
}
