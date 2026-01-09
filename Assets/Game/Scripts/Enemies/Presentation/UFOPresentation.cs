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
        [SerializeField] private Transform[] _firepoints;

        private UFOLogic _logic;
        private SpriteRotator _spriteRotator;
        private UFOSettings _settings;
        
        public Transform[] Firepoints => _firepoints;
        
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
