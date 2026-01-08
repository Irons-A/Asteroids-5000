using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Configuration;
using Core.Configuration.Player;
using Core.Physics;
using Core.Signals;
using Core.Systems;
using Core.Systems.ObjectPools;
using Player.Logic.Weapons;
using Player.Presentation;
using Player.Signals;
using Player.UserInput;
using UI.PlayerMVVM;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerControlLogic
    {
        //private const float MinEngineParticlesToggleDelay = 0.1f;
        
        private readonly PlayerShipSettings _shipSettings;
        //private readonly PlayerWeaponsSettings _weaponsSettings; 
        // private readonly UniversalPlayerWeaponSystem _bulletWeaponSystem;
        // private readonly PlayerWeaponConfig _bulletWeaponConfig;
        // private readonly UniversalPlayerWeaponSystem _laserWeaponSystem;
        // private readonly PlayerWeaponConfig _laserWeaponConfig;
        //private readonly HealthSystem _healthSystem;
        // private readonly InvulnerabilityLogic _invulnerabilityLogic;
        // private readonly UncontrollabilityLogic _uncontrollabilityLogic;
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

        //private float _lastParticleToggleTime = 0f;
        private bool _isConfigured = false;
        
        public PlayerControlLogic(JsonConfigProvider configProvider, PlayerHealthLogic healthLogic, PlayerMovementLogic movementLogic, 
            PlayerWeaponsLogic weaponsLogic, CustomPhysics playerPhysics, PlayerUIModel playerUIModel, 
            SignalBus signalBus, ParticleService particleService)
        {
            _shipSettings = configProvider.PlayerShipSettingsRef;
            //_weaponsSettings = configProvider.PlayerWeaponsSettingsRef;

            _playerPhysics = playerPhysics;

            //_bulletWeaponSystem = bulletWeapon;
            //_bulletWeaponConfig = bulletWeaponConfig;

            //_laserWeaponSystem = laserWeapon;
            //_laserWeaponConfig = laserWeaponConfig;
            
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
            
            //_playerCollisionHandler.OnRicochetCalled += _playerPhysics.ApplyRicochet;
            //_playerCollisionHandler.OnDamageReceived += TakeDamage;
            
            //_playerEngineParticles = _playerPresentation.EngineParticles;
            //_playerEngineParticles.Stop();
            
            //_invulnerabilityLogic.OnInvulnerabilityEnded += EnableCollisions;
            
            //_playerPhysics.SetMovableObject(_playerPresentation, _shipSettings.PlayerMass);
            
            // ConfigureBulletWeaponSystem();
            // ConfigureLaserWeaponSystem();
            
            //_invulnerabilityLogic.Configure(_playerSpriteRenderer, _shipSettings.InvulnerabilityDuration);
            //_uncontrollabilityLogic.Configure(_shipSettings.UncontrollabilityDuration);
            
            _playerPresentation.gameObject.SetActive(false);
            _movementLogic.StopMovement();

            _isConfigured = true;
        }

        public void Tick()
        {
            UpdateUI();
        }

        // public void FixedTick()
        // {
        //     _playerPhysics.ProcessPhysics();
        // }

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

        // private void EnableCollisions()
        // {
        //     _playerCollisionHandler.SetShouldProcessCollisions(true);
        // }
        
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
            
                //Zenject bug: Tick is not working in subclasses if there are any bool checks. Calling from here.
                // _bulletWeaponSystem.Tick();
                // _laserWeaponSystem.Tick();
            }
        }

        // private void TakeDamage(int damage)
        // {
        //     if (_invulnerabilityLogic.IsInvulnerable) return;
        //     
        //     _healthSystem.TakeDamage(damage);
        //     _playerCollisionHandler.SetShouldProcessCollisions(false);
        //     
        //     _uncontrollabilityLogic.StartUncontrollabilityPeriod();
        //     _invulnerabilityLogic.StartInvulnerabilityPeriod();
        //     
        //     _playerEngineParticles.Stop();
        //     _lastParticleToggleTime = Time.time;
        // }

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

        // private void ConfigureBulletWeaponSystem()
        // {
        //     _bulletWeaponConfig.Configure(
        //         projectileType: PoolableObjectType.PlayerBullet,
        //         firepoints: _playerPresentation.BulletFirepoints,
        //         projectileSpeed: _weaponsSettings.BulletSpeed,
        //         projectileDelayedDestruction: false,
        //         destroyProjectileAfter: 0,
        //         projectileDamage: _weaponsSettings.BulletDamage,
        //         projectileAffiliation: EntityAffiliation.Ally,
        //         projectileDurability: EntityDurability.Fragile,
        //         shouldSetFirepointAsProjectileParent: false,
        //         fireRateInterval: _weaponsSettings.BulletFireRateInterval,
        //         maxAmmo: 0,
        //         ammoCostPerShot: 0,
        //         hasInfiniteAmmo: true,
        //         reloadLength: 0,
        //         ammoPerReload: 0,
        //         shouldAutoReloadOnLessThanMaxAmmo: false,
        //         shouldAutoReloadOnNoAmmo: false,
        //         shouldDepleteAmmoOnReload: false,
        //         shouldBlockFireWhileReload: false);
        //
        //     _bulletWeaponSystem.Configure(_bulletWeaponConfig);
        // }
        //
        // private void ConfigureLaserWeaponSystem()
        // {
        //     _laserWeaponConfig.Configure(
        //         projectileType: PoolableObjectType.PlayerLaser,
        //         firepoints: _playerPresentation.LaserFirepoints,
        //         projectileSpeed: 0,
        //         projectileDelayedDestruction: true,
        //         destroyProjectileAfter: _weaponsSettings.LaserDuration,
        //         projectileDamage: _weaponsSettings.LaserDamage,
        //         projectileAffiliation: EntityAffiliation.Ally,
        //         projectileDurability: EntityDurability.Undestructable,
        //         shouldSetFirepointAsProjectileParent: true,
        //         fireRateInterval: _weaponsSettings.LaserFireRateInterval,
        //         maxAmmo: _weaponsSettings.MaxLaserCharges,
        //         ammoCostPerShot: _weaponsSettings.LaserAmmoPerShot,
        //         hasInfiniteAmmo: false,
        //         reloadLength: _weaponsSettings.LaserCooldown,
        //         ammoPerReload: _weaponsSettings.LaserAmmoPerReload,
        //         shouldAutoReloadOnLessThanMaxAmmo: true,
        //         shouldAutoReloadOnNoAmmo: true,
        //         shouldDepleteAmmoOnReload: false,
        //         shouldBlockFireWhileReload: false);
        //
        //     _laserWeaponSystem.Configure(_laserWeaponConfig);
        // }

        public void Dispose()
        {
            // if (_playerCollisionHandler != null)
            // {
            //     _playerCollisionHandler.OnRicochetCalled -= _playerPhysics.ApplyRicochet;
            //     _playerCollisionHandler.OnDamageReceived -= TakeDamage;
            // }
            
            // _healthSystem.OnHealthDepleted -= DefeatPlayer;
            // _invulnerabilityLogic.OnInvulnerabilityEnded -= EnableCollisions;
            _signalBus.Unsubscribe<ResetPlayerSignal>(ResetPlayer);
            _signalBus.Unsubscribe<DisablePlayerSignal>(DisablePlayer);
        }
    }
}
