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

            _bulletWeaponSystem.DebugInfo();
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
            _bulletWeaponConfig.ProjectileType = PoolableObjectType.PlayerBullet;
            _bulletWeaponConfig.FirePoints = _playerPresentation.BulletFirepoints;
            _bulletWeaponConfig.ProjectileSpeed = _playerSettings.BulletSpeed;
            _bulletWeaponConfig.ProjectileDelayedDestruction = false;
            _bulletWeaponConfig.DestroyProjectileAfter = 0;
            _bulletWeaponConfig.ProjectileDamage = _playerSettings.BulletDamage;
            _bulletWeaponConfig.ProjectileAffiliation = Core.Projectiles.DamagerAffiliation.Ally;
            _bulletWeaponConfig.ProjectileDurability = Core.Projectiles.DamagerDurability.Fragile;
            _bulletWeaponConfig.ShouldSetFirepointAsProjectileParent = false;
            _bulletWeaponConfig.FireRateInterval = _playerSettings.BulletFireRateInterval;
            _bulletWeaponConfig.MaxAmmo = 0;
            _bulletWeaponConfig.AmmoCostPerShot = 0;
            _bulletWeaponConfig.HasInfiniteAmmo = true;
            _bulletWeaponConfig.ReloadLength = 0;
            _bulletWeaponConfig.AmmoPerReload = 0;
            _bulletWeaponConfig.ShouldAutoReloadOnLessThanMaxAmmo = false;
            _bulletWeaponConfig.ShouldAutoReloadOnNoAmmo = false;
            _bulletWeaponConfig.ShouldDepleteAmmoOnReload = false;
            _bulletWeaponConfig.ShouldBlockFireWhileReload = false;

            _bulletWeaponSystem.Configure(_bulletWeaponConfig);
        }

        private void ConfigureLaserWeaponSystem()
        {
            _laserWeaponConfig.ProjectileType = PoolableObjectType.PlayerLaser;
            _laserWeaponConfig.FirePoints = _playerPresentation.LaserFirepoints;
            _laserWeaponConfig.ProjectileSpeed = 0;
            _laserWeaponConfig.ProjectileDelayedDestruction = true;
            _laserWeaponConfig.DestroyProjectileAfter = _playerSettings.LaserDuration;
            _laserWeaponConfig.ProjectileDamage = _playerSettings.LaserDamage;
            _laserWeaponConfig.ProjectileAffiliation = Core.Projectiles.DamagerAffiliation.Ally;
            _laserWeaponConfig.ProjectileDurability = Core.Projectiles.DamagerDurability.Undestructable;
            _laserWeaponConfig.ShouldSetFirepointAsProjectileParent = true;
            _laserWeaponConfig.FireRateInterval = _playerSettings.LaserFireRateInterval;
            _laserWeaponConfig.MaxAmmo = _playerSettings.MaxLaserCharges;
            _laserWeaponConfig.AmmoCostPerShot = 1;
            _laserWeaponConfig.HasInfiniteAmmo = false;
            _laserWeaponConfig.ReloadLength = _playerSettings.LaserCooldown;
            _laserWeaponConfig.AmmoPerReload = 1;
            _laserWeaponConfig.ShouldAutoReloadOnLessThanMaxAmmo = true;
            _laserWeaponConfig.ShouldAutoReloadOnNoAmmo = true;
            _laserWeaponConfig.ShouldDepleteAmmoOnReload = false;
            _laserWeaponConfig.ShouldBlockFireWhileReload = false;

            _laserWeaponSystem.Configure(_laserWeaponConfig);
        }
    }
}
