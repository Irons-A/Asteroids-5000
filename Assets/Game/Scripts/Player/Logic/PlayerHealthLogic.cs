using System;
using Core.Components;
using Core.Configuration;
using Core.Configuration.Player;
using Core.Systems;
using UnityEngine;

namespace Player.Logic
{
    public class PlayerHealthLogic : IDisposable
    {
        private readonly PlayerShipSettings _shipSettings;
        private readonly HealthSystem _healthSystem;
        private readonly InvulnerabilityLogic _invulnerabilityLogic;
        private readonly PlayerMovementLogic _movementLogic;
        
        private CollisionHandler _playerCollisionHandler;
        private SpriteRenderer _playerSpriteRenderer;
        
        private bool _isConfigured = false;
        
        public event Action OnNoHealthLeft;
        
        public int CurrentHealth => _healthSystem.CurrentHealth;

        public PlayerHealthLogic(JsonConfigProvider configProvider, HealthSystem healthSystem,
            InvulnerabilityLogic invulnerabilityLogic, PlayerMovementLogic playerMovementLogic)
        {
            _shipSettings = configProvider.PlayerShipSettingsRef;
            
            _healthSystem = healthSystem;
            _healthSystem.Configure(_shipSettings.MaxHealth, true);
            _healthSystem.OnHealthDepleted += FireNoHealthEvent;
            
            _invulnerabilityLogic = invulnerabilityLogic;
            _movementLogic = playerMovementLogic;
        }

        public void Configure(CollisionHandler collisionHandler, SpriteRenderer playerSpriteRenderer)
        {
            _playerCollisionHandler = collisionHandler;
            _playerCollisionHandler.OnDamageReceived += TakeDamage;
            
            _playerSpriteRenderer = playerSpriteRenderer;
            
            _invulnerabilityLogic.OnInvulnerabilityEnded += EnableCollisions;
            _invulnerabilityLogic.Configure(_playerSpriteRenderer, _shipSettings.InvulnerabilityDuration);

            _isConfigured = true;
        }

        public void RestoreHealth()
        {
            if (_isConfigured == false) return;
            
            _healthSystem.Configure(_shipSettings.MaxHealth, true);
        }

        public void StopInvulnerabilityPeriod()
        {
            if (_isConfigured == false) return;
            
            _invulnerabilityLogic.StopInvulnerabilityPeriod();
        }

        private void FireNoHealthEvent()
        {
            OnNoHealthLeft?.Invoke();
        }
        
        private void EnableCollisions()
        {
            _playerCollisionHandler.SetShouldProcessCollisions(true);
        }
        
        private void TakeDamage(int damage)
        {
            if (_invulnerabilityLogic.IsInvulnerable || _isConfigured == false) return;
            
            _healthSystem.TakeDamage(damage);
            _playerCollisionHandler.SetShouldProcessCollisions(false);
            
            _movementLogic.StartUncontrollabilityPeriod();
            _invulnerabilityLogic.StartInvulnerabilityPeriod();
            
            _movementLogic.ResetEngineParticlesToggleTime();
        }

        public void Dispose()
        {
            if (_playerCollisionHandler != null)
            {
                _playerCollisionHandler.OnDamageReceived -= TakeDamage;
            }
            
            _healthSystem.OnHealthDepleted -= FireNoHealthEvent;
            _invulnerabilityLogic.OnInvulnerabilityEnded -= EnableCollisions;
        }
    }
}
