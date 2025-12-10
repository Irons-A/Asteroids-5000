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
    public class PlayerWeaponSystem : ITickable
    {
        private PoolableObjectType _projectileType;
        private Transform[] _firePoints;
        private UniversalObjectPool _objectPool;

        private bool _shouldShoot = false;
        private bool _wasShootingLastFrame = false;
        private WeaponState _weaponState = WeaponState.Idle;
        private CancellationTokenSource _shootingCTS;
        private CancellationTokenSource _reloadCTS;

        private bool _wantsToShootButNoAmmo = false;
        private bool _isReloadingInProgress = false;

        private float _projectileSpeed;
        private bool _projectileDelayedDestruction;
        private float _destroyProjectileAfter;
        private int _projectileDamage;
        private DamagerAffiliation _projectileAffiliation;
        private DamagerDurability _projectileDurability;
        private bool _shouldSetFirepointAsProjectileParent;

        private float _fireRateInterval;
        private int _maxAmmo;
        private int _currentAmmo;
        private int _ammoCostPerShot;
        private bool _hasInfiniteAmmo;
        private float _reloadLength;
        private int _ammoPerReload;
        private bool _shouldAutoReloadOnLessThanMaxAmmo;
        private bool _shouldAutoReloadOnNoAmmo;
        private bool _shouldDepleteAmmoOnReload;
        private bool _shouldBlockFireWhileReload;

        [Inject]
        private void Construct(UniversalObjectPool objectPool)
        {
            _objectPool = objectPool;
        }

        public void Configure(PoolableObjectType projectileType, Transform[] firepoints, float projectileSpeed,
            bool projectileDelayedDestruction, float destroyProjectileAfter, int projectileDamage,
            DamagerAffiliation projectileAffiliation, DamagerDurability projectileDurability,
            bool shouldSetFirepointAsProjectileParent, float fireRateInterval, int maxAmmo, int ammoCostPerShot,
            bool hasInfiniteAmmo, float reloadLength, int ammoPerReload, bool shouldAutoReloadOnLessThanMaxAmmo,
            bool shouldAutoReloadOnNoAmmo, bool shouldDepleteAmmoOnReload, bool shouldBlockFireWhileReaload)
        {
            _projectileType = projectileType;
            _firePoints = firepoints;
            _projectileSpeed = projectileSpeed;
            _projectileDelayedDestruction = projectileDelayedDestruction;
            _destroyProjectileAfter = destroyProjectileAfter;
            _projectileDamage = projectileDamage;
            _projectileAffiliation = projectileAffiliation;
            _projectileDurability = projectileDurability;
            _shouldSetFirepointAsProjectileParent = shouldSetFirepointAsProjectileParent;
            _fireRateInterval = fireRateInterval;
            _maxAmmo = maxAmmo;
            _currentAmmo = maxAmmo;
            _ammoCostPerShot = ammoCostPerShot;
            _hasInfiniteAmmo = hasInfiniteAmmo;
            _reloadLength = reloadLength;
            _ammoPerReload = ammoPerReload;
            _shouldAutoReloadOnLessThanMaxAmmo = shouldAutoReloadOnLessThanMaxAmmo;
            _shouldAutoReloadOnNoAmmo = shouldAutoReloadOnNoAmmo;
            _shouldDepleteAmmoOnReload = shouldDepleteAmmoOnReload;
            _shouldBlockFireWhileReload = shouldBlockFireWhileReaload;
        }

        public void Tick()
        {
            if (_shouldShoot != _wasShootingLastFrame)
            {
                if (_shouldShoot)
                {
                    TryStartShooting();
                }
                else
                {
                    TryStopShooting();
                }

                _wasShootingLastFrame = _shouldShoot;
            }

            if (_shouldShoot && !CheckCanShoot() && !_hasInfiniteAmmo && _currentAmmo < _ammoCostPerShot)
            {
                _wantsToShootButNoAmmo = true;
            }
            else
            {
                _wantsToShootButNoAmmo = false;
            }

            ProcessAutoReloading();

            if (_wantsToShootButNoAmmo && CheckCanShoot() && _weaponState != WeaponState.Shooting)
            {
                TryStartShooting();
            }
        }

        private bool CheckCanShoot()
        {
            bool hasEnoughAmmo = _hasInfiniteAmmo || _currentAmmo >= _ammoCostPerShot;

            bool notBlockedByReload = !(_weaponState == WeaponState.Reloading && _shouldBlockFireWhileReload);

            bool hasFirepoints = _firePoints != null && _firePoints.Length > 0;

            return hasEnoughAmmo && notBlockedByReload && hasFirepoints;
        }

        private void TryStartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            bool canShoot = CheckCanShoot();

            if (canShoot)
            {
                StartShooting();
            }
        }

        private void TryStopShooting()
        {
            if (_weaponState == WeaponState.Shooting)
            {
                StopShooting();
            }
        }

        public void DebugInfo()
        {
            Debug.Log($"ammo: {_currentAmmo}/{_maxAmmo}, infinite: {_hasInfiniteAmmo} " +
                $"shouldShoot: {_shouldShoot}, weaponState: {_weaponState}" +
                $" IsReloadingInProgress {_isReloadingInProgress}");
        }

        public void SetShouldShoot(bool value)
        {
            if (value == _shouldShoot) return;

            _shouldShoot = value;

            if (_shouldShoot)
            {
                TryStartShooting();
            }
            else
            {
                TryStopShooting();
                _wantsToShootButNoAmmo = false; 
            }
        }

        public void StartReload()
        {
            if (_currentAmmo == _maxAmmo || _hasInfiniteAmmo)
            {
                return;
            }

            // ќстанавливаем стрельбу только если перезар€дка блокирует стрельбу
            if (_shouldBlockFireWhileReload && _weaponState == WeaponState.Shooting)
            {
                StopShooting();
            }

            // ”станавливаем состо€ние только если перезар€дка блокирует стрельбу
            if (_shouldBlockFireWhileReload)
            {
                _weaponState = WeaponState.Reloading;
            }

            // ≈сли перезар€дка уже идет, не запускаем новую
            if (_isReloadingInProgress)
            {
                return;
            }

            _reloadCTS?.Cancel();
            _reloadCTS?.Dispose();
            _reloadCTS = new CancellationTokenSource();

            ReloadLoop(_reloadCTS.Token).Forget();
        }

        private void ProcessAutoReloading()
        {
            if (_hasInfiniteAmmo)
            {
                return;
            }

            // ѕровер€ем, нужно ли начинать перезар€дку
            bool shouldStartReload = false;

            if (_shouldAutoReloadOnNoAmmo && _currentAmmo <= 0)
            {
                shouldStartReload = true;
            }
            else if (_shouldAutoReloadOnLessThanMaxAmmo && _currentAmmo < _maxAmmo)
            {
                shouldStartReload = true;
            }

            // ≈сли перезар€дка не блокирует стрельбу, используем _isReloadingInProgress
            // ≈сли блокирует - используем WeaponState.Reloading
            bool isCurrentlyReloading = _shouldBlockFireWhileReload
                ? _weaponState == WeaponState.Reloading
                : _isReloadingInProgress;

            if (shouldStartReload && !isCurrentlyReloading)
            {
                StartReload();
            }
        }

        private void StartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            _weaponState = WeaponState.Shooting;

            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = new CancellationTokenSource();

            ShootingLoop(_shootingCTS.Token).Forget();
        }

        private void StopShooting()
        {
            if (_weaponState != WeaponState.Shooting) return;

            Debug.Log("StopShooting");
            _weaponState = WeaponState.Idle;

            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = null;
        }

        private async UniTaskVoid ShootingLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _weaponState == WeaponState.Shooting)
                {
                    if (!_shouldShoot || !CheckCanShoot())
                    {
                        break;
                    }

                    ShootProjectile();

                    if (!_hasInfiniteAmmo)
                    {
                        _currentAmmo -= _ammoCostPerShot;

                        // ѕровер€ем необходимость перезар€дки после выстрела
                        ProcessAutoReloading();
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_fireRateInterval),
                        cancellationToken: token);
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
            if (_firePoints == null || _firePoints.Length == 0)
            {
                Debug.LogError("No firepoints attached!");
                return;
            }

            foreach (Transform firepoint in _firePoints)
            {
                if (firepoint == null)
                {
                    Debug.LogError("Null firepoint found!");
                    continue;
                }

                PoolableObject poolableObject = _objectPool.GetFromPool(_projectileType);

                if (poolableObject == null)
                {
                    Debug.LogError($"Failed to get projectile from pool: {_projectileType}");
                    continue;
                }

                if (poolableObject.TryGetComponent(out Projectile projectile))
                {
                    projectile.Configure(_projectileSpeed, _projectileDelayedDestruction, _destroyProjectileAfter);
                }
                else
                {
                    Debug.LogError($"Selected _projectileType {_projectileType} does not have Projectile Component");
                    continue;
                }

                if (poolableObject.TryGetComponent(out DamageDealer damageDealer))
                {
                    damageDealer.Configure(_projectileDamage, _projectileAffiliation, _projectileDurability);
                }
                else
                {
                    Debug.LogError($"Selected _projectileType {_projectileType} does not have DamageDealer Component");
                    continue;
                }

                poolableObject.transform.position = firepoint.position;
                poolableObject.transform.rotation = firepoint.rotation;

                if (_shouldSetFirepointAsProjectileParent)
                {
                    poolableObject.transform.SetParent(firepoint);
                }
            }
        }

        private async UniTaskVoid ReloadLoop(CancellationToken token)
        {
            _isReloadingInProgress = true;

            try
            {
                if (_shouldDepleteAmmoOnReload)
                {
                    _currentAmmo = 0;
                }

                // ÷икл перезар€дки - продолжаем пока есть патроны дл€ перезар€дки
                // и не достигли максимума
                while (!token.IsCancellationRequested && _currentAmmo < _maxAmmo)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_reloadLength),
                        cancellationToken: token);

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    // ƒобавл€ем патроны
                    _currentAmmo += _ammoPerReload;

                    // Ќе превышаем максимум
                    if (_currentAmmo > _maxAmmo)
                    {
                        _currentAmmo = _maxAmmo;
                    }

                    Debug.Log($"Reloaded: {_currentAmmo}/{_maxAmmo}");

                    // ≈сли патроны еще не полные, продолжаем перезар€дку
                    // (цикл продолжитс€ автоматически)
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Reload cancelled");
            }
            finally
            {
                if (_shouldBlockFireWhileReload && _weaponState == WeaponState.Reloading)
                {
                    _weaponState = WeaponState.Idle;
                }

                _isReloadingInProgress = false;

                // ѕосле завершени€ перезар€дки провер€ем, нужно ли возобновить стрельбу
                if (_shouldShoot && CheckCanShoot())
                {
                    Debug.Log("Auto-starting shooting after reload");
                    TryStartShooting();
                }

                // ≈сли после перезар€дки все еще не достигли максимума и есть услови€
                // дл€ автоперезар€дки, запускаем новую перезар€дку
                if (!token.IsCancellationRequested &&
                    _currentAmmo < _maxAmmo &&
                    (_shouldAutoReloadOnLessThanMaxAmmo ||
                     (_shouldAutoReloadOnNoAmmo && _currentAmmo <= 0)))
                {
                    // Ќебольша€ задержка перед следующей проверкой
                    await UniTask.Yield();
                    ProcessAutoReloading();
                }
            }
        }

        public void Dispose()
        {
            StopShooting();
            CancelReload();

            _shootingCTS?.Dispose();
            _reloadCTS?.Dispose();
        }

        public void CancelReload()
        {
            if (!_isReloadingInProgress && _weaponState != WeaponState.Reloading)
                return;

            Debug.Log("CancelReload");
            _reloadCTS?.Cancel();

            if (_shouldBlockFireWhileReload)
            {
                _weaponState = WeaponState.Idle;
            }

            _isReloadingInProgress = false;

            if (_shouldShoot && CheckCanShoot())
            {
                TryStartShooting();
            }
        }
    }
}
