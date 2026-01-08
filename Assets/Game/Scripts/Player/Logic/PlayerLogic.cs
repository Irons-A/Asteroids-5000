using System;
using Core.Configuration;
using Core.Physics;
using Core.Systems.ObjectPools;
using Player.Logic.Weapons;
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
    public class PlayerLogic : ITickable, IFixedTickable, IDisposable
    {
        private const float MinEngineParticlesToggleDelay = 0.1f;
        
        private readonly PlayerShipSettings _shipSettings;
        private readonly PlayerWeaponsSettings _weaponsSettings; 
        private readonly CustomPhysics _playerPhysics;
        private readonly UniversalPlayerWeaponSystem _bulletWeaponSystem;
        private readonly PlayerWeaponConfig _bulletWeaponConfig;
        private readonly UniversalPlayerWeaponSystem _laserWeaponSystem;
        private readonly PlayerWeaponConfig _laserWeaponConfig;
        private readonly HealthSystem _healthSystem;
        private readonly InvulnerabilityLogic _invulnerabilityLogic;
        private readonly UncontrollabilityLogic _uncontrollabilityLogic;
        private readonly PlayerUIModel _playerUIModel;
        private readonly SignalBus _signalBus;
        private readonly ParticleService _particleService;
        
        private PlayerPresentation _playerPresentation;
        private Transform _playerTransform;
        private CollisionHandler _playerCollisionHandler;
        private SpriteRenderer _playerSpriteRenderer;
        private ParticleSystem _playerEngineParticles;

        private float _lastParticleToggleTime = 0f;
        private bool _isConfigured = false;
        
        public PlayerLogic(JsonConfigProvider configProvider, CustomPhysics playerPhysics,
            UniversalPlayerWeaponSystem bulletWeapon, PlayerWeaponConfig bulletWeaponConfig,
            UniversalPlayerWeaponSystem laserWeapon, PlayerWeaponConfig laserWeaponConfig, HealthSystem  healthSystem,
            InvulnerabilityLogic invulnerabilityLogic, UncontrollabilityLogic  uncontrollabilityLogic,
            PlayerUIModel playerUIModel, SignalBus signalBus, ParticleService particleService)
        {
            _shipSettings = configProvider.PlayerShipSettingsRef;
            _weaponsSettings = configProvider.PlayerWeaponsSettingsRef;

            _playerPhysics = playerPhysics;

            _bulletWeaponSystem = bulletWeapon;
            _bulletWeaponConfig = bulletWeaponConfig;

            _laserWeaponSystem = laserWeapon;
            _laserWeaponConfig = laserWeaponConfig;

            _healthSystem = healthSystem;
            _healthSystem.Configure(_shipSettings.MaxHealth, true);
            _healthSystem.OnHealthDepleted += DefeatPlayer;
            
            _uncontrollabilityLogic = uncontrollabilityLogic;
            _invulnerabilityLogic = invulnerabilityLogic;
            
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
            
            _playerCollisionHandler = _playerPresentation.GetComponent<CollisionHandler>();
            _playerCollisionHandler.Configure(0, EntityAffiliation.Ally, EntityDurability.Undestructable,
                shouldCauseRicochet: true, customPhysics: _playerPhysics);
            _playerCollisionHandler.OnRicochetCalled += _playerPhysics.ApplyRicochet;
            _playerCollisionHandler.OnDamageReceived += TakeDamage;
            
            _playerSpriteRenderer = _playerPresentation.GetComponent<SpriteRenderer>();
            _invulnerabilityLogic.OnInvulnerabilityEnded += EnableCollisions;
            
            _playerPhysics.SetMovableObject(_playerPresentation, _shipSettings.PlayerMass);
            
            ConfigureBulletWeaponSystem();
            ConfigureLaserWeaponSystem();
            
            _invulnerabilityLogic.Configure(_playerSpriteRenderer, _shipSettings.InvulnerabilityDuration);
            _uncontrollabilityLogic.Configure(_shipSettings.UncontrollabilityDuration);

            _playerEngineParticles = _playerPresentation.EngineParticles;
            _playerEngineParticles.Stop();
            
            _playerPresentation.gameObject.SetActive(false);
            _playerPhysics.Stop();

            _isConfigured = true;
        }

        public void Tick()
        {
            UpdateLogic();
        }

        public void FixedTick()
        {
            _playerPhysics.ProcessPhysics();
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
            if (_uncontrollabilityLogic.IsUncontrollable) return;
            
            if (movementState == PlayerMovementState.Accelerating)
            {
                _playerPhysics.ApplyAcceleration(_shipSettings.AccelerationSpeed, _shipSettings.MaxSpeed);
                
                if (Time.time - _lastParticleToggleTime > MinEngineParticlesToggleDelay)
                {
                    _playerEngineParticles.Play();
                    _lastParticleToggleTime = Time.time;
                }
            }
            else if (movementState == PlayerMovementState.Decelerating)
            {
                _playerPhysics.ApplyDeceleration(_shipSettings.DecelerationSpeed);

            }

            if (movementState != PlayerMovementState.Accelerating)
            {
                if (Time.time - _lastParticleToggleTime > MinEngineParticlesToggleDelay)
                {
                    _playerEngineParticles.Stop();
                    _lastParticleToggleTime = Time.time;
                }
            }
        }

        public void ShootBullets(bool value)
        {
            if (_uncontrollabilityLogic.IsUncontrollable || _playerPresentation.isActiveAndEnabled == false)
            {
                value = false;
            }
            
            _bulletWeaponSystem.SetShouldShoot(value);
        }

        public void ShootLaser(bool value)
        {
            if (_uncontrollabilityLogic.IsUncontrollable || _playerPresentation.isActiveAndEnabled == false)
            {
                value = false;
            }
            
            _laserWeaponSystem.SetShouldShoot(value);
        }

        private void EnableCollisions()
        {
            _playerCollisionHandler.SetShouldProcessCollisions(true);
        }
        
        private void UpdateLogic()
        {
            if (_isConfigured)
            {
                _playerUIModel.SetHealth(_healthSystem.CurrentHealth);
                _playerUIModel.SetCoordinates(_playerTransform.position);
                _playerUIModel.SetPlayerAngle(_playerTransform.eulerAngles.z);
                _playerUIModel.SetCurrentSpeed(_playerPhysics.CurrentSpeed);
                _playerUIModel.SetLaserAmmo(_laserWeaponSystem.CurrentAmmo);
                _playerUIModel.SetLaserCooldown(_laserWeaponSystem.ReloadProgress);
            
                //Zenject bug: Tick is not working in subclasses if there are any bool checks. Calling from here.
                _bulletWeaponSystem.Tick();
                _laserWeaponSystem.Tick();
            }
        }

        private void TakeDamage(int damage)
        {
            if (_invulnerabilityLogic.IsInvulnerable) return;
            
            _healthSystem.TakeDamage(damage);
            _playerCollisionHandler.SetShouldProcessCollisions(false);
            
            _uncontrollabilityLogic.StartUncontrollabilityPeriod();
            _invulnerabilityLogic.StartInvulnerabilityPeriod();
            
            _playerEngineParticles.Stop();
            _lastParticleToggleTime = Time.time;
        }

        private void RotatePlayerAtSpeed(Vector2 targetDirection)
        {
            if (_uncontrollabilityLogic.IsUncontrollable) return;
            
            targetDirection = targetDirection.normalized;

            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            _playerTransform.rotation = Quaternion.RotateTowards(_playerTransform.rotation,
                Quaternion.Euler(0, 0, targetAngle), _shipSettings.RotationSpeed * Time.deltaTime);
        }

        private void DisablePlayer()
        {
            _playerPresentation.gameObject.SetActive(false);
            _playerPhysics.Stop();
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
            _healthSystem.Configure(_shipSettings.MaxHealth, true);
            _bulletWeaponSystem.RefillAmmo();
            _laserWeaponSystem.RefillAmmo();
            _uncontrollabilityLogic.StopUncontrollabilityPeriod();
            _invulnerabilityLogic.StopInvulnerabilityPeriod();
            _playerPhysics.Stop();
            _playerPresentation.gameObject.SetActive(true);
        }

        private void ConfigureBulletWeaponSystem()
        {
            _bulletWeaponConfig.Configure(
                projectileType: PoolableObjectType.PlayerBullet,
                firepoints: _playerPresentation.BulletFirepoints,
                projectileSpeed: _weaponsSettings.BulletSpeed,
                projectileDelayedDestruction: false,
                destroyProjectileAfter: 0,
                projectileDamage: _weaponsSettings.BulletDamage,
                projectileAffiliation: EntityAffiliation.Ally,
                projectileDurability: EntityDurability.Fragile,
                shouldSetFirepointAsProjectileParent: false,
                fireRateInterval: _weaponsSettings.BulletFireRateInterval,
                maxAmmo: 0,
                ammoCostPerShot: 0,
                hasInfiniteAmmo: true,
                reloadLength: 0,
                ammoPerReload: 0,
                shouldAutoReloadOnLessThanMaxAmmo: false,
                shouldAutoReloadOnNoAmmo: false,
                shouldDepleteAmmoOnReload: false,
                shouldBlockFireWhileReload: false);

            _bulletWeaponSystem.Configure(_bulletWeaponConfig);
        }

        private void ConfigureLaserWeaponSystem()
        {
            _laserWeaponConfig.Configure(
                projectileType: PoolableObjectType.PlayerLaser,
                firepoints: _playerPresentation.LaserFirepoints,
                projectileSpeed: 0,
                projectileDelayedDestruction: true,
                destroyProjectileAfter: _weaponsSettings.LaserDuration,
                projectileDamage: _weaponsSettings.LaserDamage,
                projectileAffiliation: EntityAffiliation.Ally,
                projectileDurability: EntityDurability.Undestructable,
                shouldSetFirepointAsProjectileParent: true,
                fireRateInterval: _weaponsSettings.LaserFireRateInterval,
                maxAmmo: _weaponsSettings.MaxLaserCharges,
                ammoCostPerShot: _weaponsSettings.LaserAmmoPerShot,
                hasInfiniteAmmo: false,
                reloadLength: _weaponsSettings.LaserCooldown,
                ammoPerReload: _weaponsSettings.LaserAmmoPerReload,
                shouldAutoReloadOnLessThanMaxAmmo: true,
                shouldAutoReloadOnNoAmmo: true,
                shouldDepleteAmmoOnReload: false,
                shouldBlockFireWhileReload: false);

            _laserWeaponSystem.Configure(_laserWeaponConfig);
        }

        public void Dispose()
        {
            if (_playerCollisionHandler != null)
            {
                _playerCollisionHandler.OnRicochetCalled -= _playerPhysics.ApplyRicochet;
                _playerCollisionHandler.OnDamageReceived -= TakeDamage;
            }
            
            _healthSystem.OnHealthDepleted -= DefeatPlayer;
            _invulnerabilityLogic.OnInvulnerabilityEnded -= EnableCollisions;
            _signalBus.Unsubscribe<ResetPlayerSignal>(ResetPlayer);
            _signalBus.Unsubscribe<DisablePlayerSignal>(DisablePlayer);
        }
    }
}
