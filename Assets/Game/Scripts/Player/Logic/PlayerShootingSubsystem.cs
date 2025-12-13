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

namespace Player.Logic
{
    public class PlayerShootingSubsystem : ITickable
    {
        public event Action OnShotFired;

        private UniversalObjectPool _objectPool;
        private WeaponConfig _config;

        //private bool _shouldShoot = false;
        private WeaponState _weaponState = WeaponState.Idle;
        private CancellationTokenSource _shootingCTS;
        private float _lastShotTime = 0f;

        private PlayerReloadingSubsystem _reloadingSubsystem;
        private AmmoManager _ammoManager;

        private bool _isInitialized = false;

        [Inject]
        public void Construct(UniversalObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        public void Configure(WeaponConfig config, PlayerReloadingSubsystem reloadingSubsystem, AmmoManager ammoManager)
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

                    return _config.FirePoints != null && _config.FirePoints.Length > 0;
                }
                return false;
            }
        }

        //public void SetShouldShoot(bool value) //?????
        //{
        //    if (value == _shouldShoot) return;

        //    _shouldShoot = value;

        //    if (_shouldShoot)
        //    {
        //        TryStartShooting();
        //    }
        //    else
        //    {
        //        TryStopShooting();
        //    }
        //}

        public void TryStartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            Debug.Log("TryStartShooting");

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

            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = null;
        }

        private void StartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            _weaponState = WeaponState.Shooting;

            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = new CancellationTokenSource();

            ShootingLoop(_shootingCTS.Token).Forget();

            Debug.Log("StartShooting");
        }

        private async UniTaskVoid ShootingLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _weaponState == WeaponState.Shooting)
                {
                    //Debug.Log($"ShootingLoop: _shouldShoot {_shouldShoot} CanShoot {CanShoot}");

                    if (!CanShoot)
                    {
                        break;
                    }

                    Debug.Log("ShotFired");

                    // Проверка скорострельности
                    float timeSinceLastShot = Time.time - _lastShotTime;
                    if (timeSinceLastShot < _config.FireRateInterval)
                    {
                        float waitTime = _config.FireRateInterval - timeSinceLastShot;
                        await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
                        if (token.IsCancellationRequested || !CanShoot) break;
                    }

                    // Выстрел
                    _lastShotTime = Time.time;
                    ShootProjectile();

                    // Списание патронов
                    if (!_ammoManager.HasInfiniteAmmo)
                    {
                        _ammoManager.ConsumeAmmo(_config.AmmoCostPerShot);
                    }

                    // Ждем до следующего возможного выстрела
                    await UniTask.Delay(TimeSpan.FromSeconds(_config.FireRateInterval), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Shooting loop cancelled");
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

                ConfigureProjectile(poolableObject);
                PositionProjectile(poolableObject, firepoint);
            }

            OnShotFired?.Invoke();
        }

        private void ConfigureProjectile(PoolableObject poolableObject)
        {
            if (poolableObject.TryGetComponent(out Projectile projectile))
            {
                projectile.Configure(_config.ProjectileSpeed, _config.ProjectileDelayedDestruction,
                    _config.DestroyProjectileAfter);
            }

            if (poolableObject.TryGetComponent(out DamageDealer damageDealer))
            {
                damageDealer.Configure(_config.ProjectileDamage, _config.ProjectileAffiliation,
                    _config.ProjectileDurability);
            }
        }

        private void PositionProjectile(PoolableObject projectile, Transform firepoint)
        {
            projectile.transform.SetPositionAndRotation(firepoint.position, firepoint.rotation);

            if (_config.ShouldSetFirepointAsProjectileParent)
            {
                projectile.transform.SetParent(firepoint);
            }
        }

        public void Tick()
        {
            if (_isInitialized == false) return;
            // Если кнопка зажата, но не стреляем и можем стрелять - начинаем
            if (_weaponState != WeaponState.Shooting && CanShoot)
            {
                TryStartShooting();
            }
        }

        public void Dispose()
        {
            StopShooting();
        }
    }
}
