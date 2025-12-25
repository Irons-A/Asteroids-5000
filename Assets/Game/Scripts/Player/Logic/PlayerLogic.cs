using System;
using Core.Configuration;
using Core.Physics;
using Core.Systems.ObjectPools;
using Player.Logic.Weapons;
using Player.Presentation;
using Player.UserInput;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Core.Components;
using Core.Signals;
using Core.Systems;
using Cysharp.Threading.Tasks;
using Player.Signals;
using UI;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerLogic : ITickable, IFixedTickable, IInitializable, IDisposable
    {
        private PlayerPresentation _playerPresentation;
        private Transform _playerTransform;
        private PlayerSettings _playerSettings; 
        private CustomPhysics _playerPhysics;
        private PoolAccessProvider _objectPool;
        private UniversalPlayerWeaponSystem _bulletWeaponSystem;
        private PlayerWeaponConfig _bulletWeaponConfig;
        private UniversalPlayerWeaponSystem _laserWeaponSystem;
        private PlayerWeaponConfig _laserWeaponConfig;
        private CollisionHandler _playerCollisionHandler;
        private HealthSystem _healthSystem;
        private InvulnerabilityLogic _invulnerabilityLogic;
        private SpriteRenderer _playerSpriteRenderer;
        private UncontrollabilityLogic _playerUncontrollabilityLogic;
        private PlayerUIModel _playerUIModel;
        private SignalBus _signalBus;

        [Inject]
        private void Construct(PlayerPresentation playerPresentation, JsonConfigProvider configProvider,
            CustomPhysics playerPhysics, PoolAccessProvider objectPool, UniversalPlayerWeaponSystem bulletWeapon,
            PlayerWeaponConfig bulletWeaponConfig, UniversalPlayerWeaponSystem laserWeapon,
            PlayerWeaponConfig laserWeaponConfig, HealthSystem  healthSystem, InvulnerabilityLogic invulnerabilityLogic,
            UncontrollabilityLogic  uncontrollabilityLogic, PlayerUIModel playerUIModel, SignalBus signalBus)
        {
            _playerSettings = configProvider.PlayerSettingsRef;

            _playerPresentation = playerPresentation;
            _playerTransform = playerPresentation.transform;

            _playerPhysics = playerPhysics;

            _objectPool = objectPool;

            _bulletWeaponSystem = bulletWeapon;
            _bulletWeaponConfig = bulletWeaponConfig;

            _laserWeaponSystem = laserWeapon;
            _laserWeaponConfig = laserWeaponConfig;

            _healthSystem = healthSystem;
            _healthSystem.Configure(_playerSettings.MaxHealth, true);
            _healthSystem.OnHealthDepleted += DisablePlayer;
            
            _playerCollisionHandler = _playerPresentation.GetComponent<CollisionHandler>();
            _playerCollisionHandler.OnRicochetCalled += _playerPhysics.ApplyRicochet;
            _playerCollisionHandler.OnDamageReceived += TakeDamage;
            
            _invulnerabilityLogic = invulnerabilityLogic;
            _playerSpriteRenderer = _playerPresentation.GetComponent<SpriteRenderer>();
            _invulnerabilityLogic.OnInvulnerabilityEnded += EnableCollisions;
            
            _playerUncontrollabilityLogic = uncontrollabilityLogic;
            
            _playerUIModel = playerUIModel;
            
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _playerPhysics.SetMovableObject(_playerPresentation);
            
            ConfigureBulletWeaponSystem();
            ConfigureLaserWeaponSystem();
            
            _invulnerabilityLogic.Configure(_playerSpriteRenderer, _playerSettings.InvulnerabilityDuration);
            _playerUncontrollabilityLogic.Configure(_playerSettings.UncontrollabilityDuration);
            
            _signalBus.Subscribe<ResetPlayerSignal>(ResetPlayer);
        }

        public void Tick()
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
            if (_playerUncontrollabilityLogic.IsUncontrollable) return;
            
            if (movementState == PlayerMovementState.Accelerating)
            {
                _playerPhysics.ApplyAcceleration(_playerSettings.AccelerationSpeed, _playerSettings.MaxSpeed);
            }
            else if (movementState == PlayerMovementState.Decelerating)
            {
                _playerPhysics.ApplyDeceleration(_playerSettings.DecelerationSpeed);
            }
        }

        public void ShootBullets(bool value)
        {
            if (_playerUncontrollabilityLogic.IsUncontrollable)
            {
                value = false;
            }
            
            _bulletWeaponSystem.SetShouldShoot(value);
        }

        public void ShootLaser(bool value)
        {
            if (_playerUncontrollabilityLogic.IsUncontrollable)
            {
                value = false;
            }
            
            _laserWeaponSystem.SetShouldShoot(value);
        }

        private void EnableCollisions()
        {
            _playerCollisionHandler.SetShouldProcessCollisions(true);
        }

        private void TakeDamage(int damage)
        {
            if (_invulnerabilityLogic.IsInvulnerable) return;
            
            _healthSystem.TakeDamage(damage);
            _playerCollisionHandler.SetShouldProcessCollisions(false);
            
            _playerUncontrollabilityLogic.StartUncontrollabilityPeriod();
            _invulnerabilityLogic.StartInvulnerabilityPeriod();
        }

        private void RotatePlayerAtSpeed(Vector2 targetDirection)
        {
            if (_playerUncontrollabilityLogic.IsUncontrollable) return;
            
            targetDirection = targetDirection.normalized;

            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            _playerTransform.rotation = Quaternion.RotateTowards(_playerTransform.rotation,
                Quaternion.Euler(0, 0, targetAngle), _playerSettings.RotationSpeed * Time.deltaTime);
        }

        private void DisablePlayer()
        {
            _playerPresentation.gameObject.SetActive(false);
            _playerPhysics.Stop();
            _signalBus.TryFire(new EndGameSignal());
        }

        private void ResetPlayer()
        {
            _playerPresentation.transform.position = Vector3.zero;
            _healthSystem.Configure(_playerSettings.MaxHealth, true);
            _bulletWeaponSystem.RefillAmmo();
            _laserWeaponSystem.RefillAmmo();
            _playerPresentation.gameObject.SetActive(true);
        }

        private void ConfigureBulletWeaponSystem()
        {
            _bulletWeaponConfig.Configure(
                projectileType: PoolableObjectType.PlayerBullet,
                firepoints: _playerPresentation.BulletFirepoints,
                projectileSpeed: _playerSettings.BulletSpeed,
                projectileDelayedDestruction: false,
                destroyProjectileAfter: 0,
                projectileDamage: _playerSettings.BulletDamage,
                projectileAffiliation: EntityAffiliation.Ally,
                projectileDurability: EntityDurability.Fragile,
                shouldSetFirepointAsProjectileParent: false,
                fireRateInterval: _playerSettings.BulletFireRateInterval,
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
                destroyProjectileAfter: _playerSettings.LaserDuration,
                projectileDamage: _playerSettings.LaserDamage,
                projectileAffiliation: EntityAffiliation.Ally,
                projectileDurability: EntityDurability.Undestructable,
                shouldSetFirepointAsProjectileParent: true,
                fireRateInterval: _playerSettings.LaserFireRateInterval,
                maxAmmo: _playerSettings.MaxLaserCharges,
                ammoCostPerShot: _playerSettings.LaserAmmoPerShot,
                hasInfiniteAmmo: false,
                reloadLength: _playerSettings.LaserCooldown,
                ammoPerReload: _playerSettings.LaserAmmoPerReload,
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
            
            _healthSystem.OnHealthDepleted -= DisablePlayer;
            _invulnerabilityLogic.OnInvulnerabilityEnded -= EnableCollisions;
            _signalBus.Unsubscribe<ResetPlayerSignal>(ResetPlayer);
        }
    }
}
