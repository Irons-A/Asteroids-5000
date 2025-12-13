using Core.Components;
using Core.Projectiles;
using Core.Systems.ObjectPools;
using Core.Weapons;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Player.Logic.Weapons
{
    public class PlayerShootingSubsystem : ITickable
    {
        private UniversalObjectPool _objectPool;
        private PlayerWeaponConfig _config;

        private WeaponState _weaponState = WeaponState.Idle;
        private CancellationTokenSource _shootingCTS;
        private float _lastShotTime = 0f;

        private PlayerReloadingSubsystem _reloadingSubsystem;
        private PlayerAmmoSubsystem _ammoManager;

        private bool _isInitialized = false;

        public event Action OnShotFired;

        [Inject]
        public void Construct(UniversalObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        public void Configure(PlayerWeaponConfig config, PlayerReloadingSubsystem reloadingSubsystem, PlayerAmmoSubsystem ammoManager)
        {
            _config = config;
            _reloadingSubsystem = reloadingSubsystem;
            _ammoManager = ammoManager;
            _ammoManager.ResetAmmo(config.MaxAmmo);

            _isInitialized = true;
        }

        public bool CanShoot
        {
            get
            {
                if (_isInitialized == false) return false;

                if (_ammoManager.HasInfiniteAmmo || _ammoManager.CurrentAmmo >= _config.AmmoCostPerShot)
                {
                    if (_reloadingSubsystem.IsReloading && _reloadingSubsystem.ShouldBlockFire)
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        public void TryStartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            if (CanShoot)
            {
                StartShooting();
            }
        }

        public void TryStopShooting()
        {
            if (_weaponState == WeaponState.Shooting)
            {
                StopShooting();
            }
        }

        public void StopShooting()
        {
            if (_weaponState != WeaponState.Shooting) return;

            _weaponState = WeaponState.Idle;

            DisposeShootingCancellationToken();
        }

        public void Tick()
        {
            if (_isInitialized == false) return;

            if (_weaponState != WeaponState.Shooting && CanShoot)
            {
                TryStartShooting();
            }
        }

        public void Dispose()
        {
            StopShooting();
        }

        private void StartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            _weaponState = WeaponState.Shooting;

            DisposeShootingCancellationToken();

            _shootingCTS = new CancellationTokenSource();

            ShootingLoop(_shootingCTS.Token).Forget();
        }

        private async UniTaskVoid ShootingLoop(CancellationToken token)
        {
            try
            {
                while (token.IsCancellationRequested == false && _weaponState == WeaponState.Shooting)
                {
                    if (CanShoot == false)
                    {
                        break;
                    }

                    float timeSinceLastShot = Time.time - _lastShotTime;

                    if (timeSinceLastShot < _config.FireRateInterval)
                    {
                        float waitTime = _config.FireRateInterval - timeSinceLastShot;

                        await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);

                        if (token.IsCancellationRequested || CanShoot == false) break;
                    }

                    _lastShotTime = Time.time;

                    ShootProjectile();

                    if (_ammoManager.HasInfiniteAmmo == false)
                    {
                        _ammoManager.ConsumeAmmo(_config.AmmoCostPerShot);
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_config.FireRateInterval), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
                //Debug.Log("Shooting loop cancelled");
            }
            finally
            {
                if (_weaponState == WeaponState.Shooting)
                {
                    _weaponState = WeaponState.Idle;
                }
            }
        }

        private void ShootProjectile()
        {
            if (_config.FirePoints == null || _config.FirePoints.Length == 0)
            {
                Debug.LogError("No firepoints attached!");

                return;
            }

            foreach (Transform firepoint in _config.FirePoints)
            {
                if (firepoint == null)
                {
                    Debug.LogError("Null firepoint found!");

                    continue;
                }

                PoolableObject poolableObject = _objectPool.GetFromPool(_config.ProjectileType);

                if (poolableObject == null)
                {
                    Debug.LogError($"Failed to get projectile from pool: {_config.ProjectileType}");

                    continue;
                }

                ConfigureProjectile(poolableObject, firepoint);
            }

            OnShotFired?.Invoke();
        }

        private void ConfigureProjectile(PoolableObject poolableObject, Transform firepoint)
        {
            if (poolableObject.TryGetComponent(out Projectile projectile))
            {
                projectile.Configure(_config.ProjectileSpeed, _config.ProjectileDelayedDestruction,
                    _config.DestroyProjectileAfter);
            }
            else
            {
                Debug.LogError("Projectile does not have Projectile component!");
            }

            if (poolableObject.TryGetComponent(out DamageDealer damageDealer))
            {
                damageDealer.Configure(_config.ProjectileDamage, _config.ProjectileAffiliation,
                    _config.ProjectileDurability);
            }
            else
            {
                Debug.LogError("Projectile does not have DamageDealer component!");
            }

            projectile.transform.SetPositionAndRotation(firepoint.position, firepoint.rotation);

            if (_config.ShouldSetFirepointAsProjectileParent)
            {
                projectile.transform.SetParent(firepoint);
            }
        }

        private void DisposeShootingCancellationToken()
        {
            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = null;
        }
    }
}
