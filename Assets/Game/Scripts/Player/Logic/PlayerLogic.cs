using System;
using Core.Configuration;
using Core.Physics;
using Core.Systems.ObjectPools;
using Player.Presentation;
using Player.UserInput;
using Core.Components;
using Core.Configuration.Player;
using Core.Signals;
using Core.Systems;
using Player.Signals;
using UI.PlayerMVVM;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerLogic : ITickable, IDisposable
    {
        private readonly PlayerShipSettings _shipSettings;
        private readonly PlayerHealthLogic _healthLogic;
        private readonly PlayerMovementLogic _movementLogic;
        private readonly PlayerWeaponsLogic _weaponsLogic;
        private readonly CustomPhysics _playerPhysics;
        private readonly PlayerUIModel _playerUIModel;
        private readonly SignalBus _signalBus;
        private readonly ParticleService _particleService;
        
        private PlayerPresentation _playerPresentation;
        private Transform _playerTransform;
        private CollisionHandler _playerCollisionHandler;
        private SpriteRenderer _playerSpriteRenderer;
        private ParticleSystem _playerEngineParticles;
        
        private bool _isConfigured = false;
        
        public PlayerLogic(JsonConfigProvider configProvider, PlayerHealthLogic healthLogic,
            PlayerMovementLogic movementLogic, PlayerWeaponsLogic weaponsLogic, CustomPhysics playerPhysics,
            PlayerUIModel playerUIModel, SignalBus signalBus, ParticleService particleService)
        {
            _shipSettings = configProvider.PlayerShipSettingsRef;

            _playerPhysics = playerPhysics;
            
            _healthLogic = healthLogic;
            healthLogic.OnNoHealthLeft += DefeatPlayer;
            
            _movementLogic = movementLogic;
            _weaponsLogic = weaponsLogic;
            
            _playerUIModel = playerUIModel;
            
            _signalBus = signalBus;
            _signalBus.Subscribe<ResetPlayerSignal>(ResetPlayer);
            _signalBus.Subscribe<DisablePlayerSignal>(DisablePlayer);

            _particleService = particleService;
        }

        public void Configure(PlayerPresentation playerPresentation)
        {
            _playerPresentation = playerPresentation;
            _playerTransform = playerPresentation.transform;
            
            _playerSpriteRenderer = _playerPresentation.GetComponent<SpriteRenderer>();
            
            _playerCollisionHandler = _playerPresentation.GetComponent<CollisionHandler>();
            _playerCollisionHandler.Configure(0, EntityAffiliation.Ally, EntityDurability.Undestructable,
                shouldCauseRicochet: true, customPhysics: _playerPhysics);
            
            _healthLogic.Configure(_playerCollisionHandler, _playerSpriteRenderer);
            _movementLogic.Configure(_playerCollisionHandler, _playerPhysics, _playerPresentation);
            _weaponsLogic.Configure(_playerPresentation);
            
            _playerPresentation.gameObject.SetActive(false);
            _movementLogic.StopMovement();

            _isConfigured = true;
        }

        public void Tick()
        {
            UpdateUI();
        }

        public void RotatePlayerWithMouse(Vector2 mouseWorldPosition)
        {
            Vector2 transformVector2 = _playerTransform.position;

            Vector2 direction = (mouseWorldPosition - transformVector2);

            RotatePlayerAtSpeed(direction);
        }

        public void RotatePlayerTowardsStick(Vector2 direction)
        {
            RotatePlayerAtSpeed(direction);
        }

        public void MovePlayer(PlayerMovementState movementState)
        {
            _movementLogic.MovePlayer(movementState);
        }

        public void ShootBullets(bool value)
        {
            _weaponsLogic.ShootBullets(value);
        }

        public void ShootLaser(bool value)
        {
            _weaponsLogic.ShootLaser(value);
        }
        
        private void UpdateUI()
        {
            if (_isConfigured)
            {
                _playerUIModel.SetHealth(_healthLogic.CurrentHealth);
                _playerUIModel.SetCoordinates(_playerTransform.position);
                _playerUIModel.SetPlayerAngle(_playerTransform.eulerAngles.z);
                _playerUIModel.SetCurrentSpeed(_playerPhysics.CurrentSpeed);
                _playerUIModel.SetLaserAmmo(_weaponsLogic.CurrentLaserAmmo);
                _playerUIModel.SetLaserCooldown(_weaponsLogic.CurrentLaserCooldownProgress);
            }
        }

        private void RotatePlayerAtSpeed(Vector2 targetDirection)
        {
            if (_movementLogic.IsUncontrollable) return;
            
            targetDirection = targetDirection.normalized;

            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            _playerTransform.rotation = Quaternion.RotateTowards(_playerTransform.rotation,
                Quaternion.Euler(0, 0, targetAngle), _shipSettings.RotationSpeed * Time.deltaTime);
        }

        private void DisablePlayer()
        {
            _playerPresentation.gameObject.SetActive(false);
            _movementLogic.StopMovement();
        }

        private void DefeatPlayer()
        {
            _particleService.SpawnParticles(PoolableObjectType.ExplosionParticles, _playerTransform.position);
            
            DisablePlayer();
            
            _signalBus.TryFire(new EndGameSignal());
        }

        private void ResetPlayer()
        {
            _playerPresentation.transform.position = Vector3.zero;
            _healthLogic.RestoreHealth();
            _weaponsLogic.RefillAmmo();
            _movementLogic.StopUncontrollabilityPeriod();
            _healthLogic.StopInvulnerabilityPeriod();
            _movementLogic.StopMovement();
            _playerPresentation.gameObject.SetActive(true);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<ResetPlayerSignal>(ResetPlayer);
            _signalBus.Unsubscribe<DisablePlayerSignal>(DisablePlayer);
        }
    }
}
