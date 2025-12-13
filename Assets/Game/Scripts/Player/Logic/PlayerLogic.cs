using Core.Configuration;
using Core.Physics;
using Core.Systems.ObjectPools;
using Player.Presentation;
using Player.UserInput;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerLogic : IFixedTickable, IInitializable
    {
        private PlayerPresentation _playerPresentation;
        private Transform _playerTransform;
        private PlayerSettings _playerSettings; 
        private CustomPhysics _playerPhysics;
        private UniversalObjectPool _objectPool;
        private UniversalPlayerWeaponSystem _bulletWeaponSystem;
        private WeaponConfig _bulletWeaponConfig;
        private UniversalPlayerWeaponSystem _laserWeaponSystem;
        private WeaponConfig _laserWeaponConfig;

        [Inject]
        private void Construct(PlayerPresentation playerView, JsonConfigProvider configProvider,
            CustomPhysics playerPhysics, UniversalObjectPool objectPool, UniversalPlayerWeaponSystem bulletWeapon,
            WeaponConfig bulletWeaponConfig, UniversalPlayerWeaponSystem laserWeapon, WeaponConfig laserWeaponConfig)
        {
            _playerSettings = configProvider.PlayerSettingsRef;

            _playerPresentation = playerView;
            _playerTransform = playerView.transform;

            _playerPhysics = playerPhysics;
            _playerPhysics.SetMovableObject(playerView);

            _objectPool = objectPool;

            _bulletWeaponSystem = bulletWeapon;
            _bulletWeaponConfig = bulletWeaponConfig;

            _laserWeaponSystem = laserWeapon;
            _laserWeaponConfig = laserWeaponConfig;
        }

        public void Initialize()
        {
            ConfigureBulletWeaponSystem();
            ConfigureLaserWeaponSystem();
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
            _bulletWeaponSystem.SetShouldShoot(value);
        }

        public void ShootLaser(bool value)
        {
            _laserWeaponSystem.SetShouldShoot(value);
        }

        public void TogglePause()
        {

        }

        private void RotatePlayerAtSpeed(Vector2 targetDirection)
        {
            targetDirection = targetDirection.normalized;

            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            _playerTransform.rotation = Quaternion.RotateTowards(_playerTransform.rotation,
                Quaternion.Euler(0, 0, targetAngle), _playerSettings.RotationSpeed * Time.deltaTime);
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
                projectileAffiliation: Core.Projectiles.DamagerAffiliation.Ally,
                projectileDurability: Core.Projectiles.DamagerDurability.Fragile,
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
                shouldBlockFireWhileReaload: false);

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
                projectileAffiliation: Core.Projectiles.DamagerAffiliation.Ally,
                projectileDurability: Core.Projectiles.DamagerDurability.Undestructable,
                shouldSetFirepointAsProjectileParent: true,
                fireRateInterval: _playerSettings.LaserFireRateInterval,
                maxAmmo: _playerSettings.MaxLaserCharges,
                ammoCostPerShot: 1,
                hasInfiniteAmmo: false,
                reloadLength: _playerSettings.LaserCooldown,
                ammoPerReload: 1,
                shouldAutoReloadOnLessThanMaxAmmo: true,
                shouldAutoReloadOnNoAmmo: true,
                shouldDepleteAmmoOnReload: false,
                shouldBlockFireWhileReaload: false);

            _laserWeaponSystem.Configure(_laserWeaponConfig);
        }
    }
}
