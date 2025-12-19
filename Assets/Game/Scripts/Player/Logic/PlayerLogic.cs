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
using Core.Systems;
using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerLogic : IFixedTickable, IInitializable, IDisposable
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
        
        private CancellationTokenSource _uncontrollabilityCTS;
        private bool _isUncontrollable;

        private CancellationTokenSource _invulnerabilityCTS;
        private bool _isInvulnerable;

        [Inject]
        private void Construct(PlayerPresentation playerPresentation, JsonConfigProvider configProvider,
            CustomPhysics playerPhysics, PoolAccessProvider objectPool, UniversalPlayerWeaponSystem bulletWeapon,
            PlayerWeaponConfig bulletWeaponConfig, UniversalPlayerWeaponSystem laserWeapon,
            PlayerWeaponConfig laserWeaponConfig, HealthSystem  healthSystem)
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
            
            _playerCollisionHandler = _playerPresentation.GetComponent<CollisionHandler>();
            _playerCollisionHandler.OnRicochetCalled += _playerPhysics.ApplyRicochet;
            _playerCollisionHandler.OnDamageReceived += TakeDamage;
        }

        public void Initialize()
        {
            _playerPhysics.SetMovableObject(_playerPresentation);
            
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
            if (_isUncontrollable)  return;
            
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
            if (_isUncontrollable)  return;
            
            _bulletWeaponSystem.SetShouldShoot(value);
        }

        public void ShootLaser(bool value)
        {
            if (_isUncontrollable)  return;
            
            _laserWeaponSystem.SetShouldShoot(value);
        }

        private void TakeDamage(int damage)
        {
            if (_isInvulnerable) return;
            
            _healthSystem.TakeDamage(damage);
            
            StartUncontrollabilityPeriod();
            StartInvulnerabilityPeriod();
        }

        private void RotatePlayerAtSpeed(Vector2 targetDirection)
        {
            if (_isUncontrollable)  return;
            
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
                shouldBlockFireWhileReaload: false);

            _laserWeaponSystem.Configure(_laserWeaponConfig);
        }

        private void StartUncontrollabilityPeriod()
        {
            if (_isUncontrollable) return;
            
            DisposeUncontrollabilityCTS();
                
            _uncontrollabilityCTS = new CancellationTokenSource();
                
            _isUncontrollable = true;

            UncontrolabilityTask(_uncontrollabilityCTS.Token).Forget();
        }

        private void DisposeUncontrollabilityCTS()
        {
            _uncontrollabilityCTS?.Cancel();
            _uncontrollabilityCTS?.Dispose();
            _uncontrollabilityCTS = null;
        }

        private async UniTaskVoid UncontrolabilityTask(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_playerSettings.UncontrollabilityDuration),
                    cancellationToken: token);

                if (_isUncontrollable == false) return;

                _isUncontrollable = false;
            }
            catch (OperationCanceledException)
            {
                _isUncontrollable = false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error in UncontrolabilityTask: {e.Message}");

                _isUncontrollable = false;
            }
        }
        
        private void StartInvulnerabilityPeriod()
        {
            if (_isInvulnerable) return;
            
            DisposeInvulnerabilityCTS();
                
            _invulnerabilityCTS = new CancellationTokenSource();
                
            _isInvulnerable = true;

            //UncontrolabilityTask(_invulnerabilityCTS.Token).Forget();
        }

        private void DisposeInvulnerabilityCTS()
        {
            _invulnerabilityCTS?.Cancel();
            _invulnerabilityCTS?.Dispose();
            _invulnerabilityCTS = null;
        }

        public void Dispose()
        {
            if (_playerCollisionHandler != null)
            {
                _playerCollisionHandler.OnRicochetCalled -= _playerPhysics.ApplyRicochet;
                _playerCollisionHandler.OnDamageReceived -= TakeDamage;
            }
        }
    }
}
