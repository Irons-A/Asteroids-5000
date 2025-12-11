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
    public class UniveralPlayerWeaponSystem : ITickable, IDisposable
    {
        private PoolableObjectType _projectileType;
        private Transform[] _firePoints;
        private UniversalObjectPool _objectPool;

        private bool _shouldShoot = false;
        private bool _isReloadingInProgress = false;
        private float _lastShotTime = 0f;
        private WeaponState _weaponState = WeaponState.Idle;
        private CancellationTokenSource _shootingCTS;
        private CancellationTokenSource _reloadCTS;

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
            ProcessAutoReloading();
        }

        public void Dispose()
        {
            StopShooting();
            CancelReload();
        }

        public void DebugInfo()
        {
            Debug.Log($"ammo: {_currentAmmo}/{_maxAmmo}, infinite: {_hasInfiniteAmmo} " +
                $"shouldShoot: {_shouldShoot}, weaponState: {_weaponState}" +
                $" IsReloadingInProgress {_isReloadingInProgress} canShoot {CheckCanShoot()}");
        }

        public void SetShouldShoot(bool value)
        {
            if (value == _shouldShoot) return;

            _shouldShoot = value;

            if (_shouldShoot && _weaponState != WeaponState.Shooting && CheckCanShoot())
            {
                TryStartShooting();
            }
            else
            {
                TryStopShooting();
            }
        }

        public void StartReload()
        {
            if (_currentAmmo == _maxAmmo || _hasInfiniteAmmo)
            {
                return;
            }

            if (_shouldBlockFireWhileReload && _weaponState == WeaponState.Shooting)
            {
                StopShooting();
            }

            if (_shouldBlockFireWhileReload)
            {
                _weaponState = WeaponState.Reloading;
            }

            if (_isReloadingInProgress)
            {
                return;
            }

            DisposeReloadingCancellationToken();

            _reloadCTS = new CancellationTokenSource();

            ReloadLoop(_reloadCTS.Token).Forget();
        }

        public void CancelReload()
        {
            if (_isReloadingInProgress == false && _weaponState != WeaponState.Reloading)
                return;

            DisposeReloadingCancellationToken();

            if (_shouldBlockFireWhileReload)
            {
                _weaponState = WeaponState.Idle;
            }

            _isReloadingInProgress = false;

            if (_shouldShoot)
            {
                TryStartShooting();
            }
        }

        private bool CheckCanShoot()
        {
            bool hasEnoughAmmo = _hasInfiniteAmmo || _currentAmmo >= _ammoCostPerShot;

            if (hasEnoughAmmo == false) return false;

            if (_isReloadingInProgress && _shouldBlockFireWhileReload)
            {
                return false;
            }

            return true;
        }

        private void TryStartShooting()
        {
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

        private void StartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            if (_isReloadingInProgress && _shouldBlockFireWhileReload) return;

            _weaponState = WeaponState.Shooting;

            DisposeShootingCancellationToken();

            _shootingCTS = new CancellationTokenSource();

            ShootingLoop(_shootingCTS.Token).Forget();
        }

        private void StopShooting()
        {
            if (_weaponState != WeaponState.Shooting) return;

            _weaponState = WeaponState.Idle;

            DisposeShootingCancellationToken();
        }

        private async UniTaskVoid ShootingLoop(CancellationToken token)
        {
            try
            {
                while (token.IsCancellationRequested == false && _weaponState == WeaponState.Shooting)
                {
                    if (token.IsCancellationRequested) break;

                    if (_shouldShoot == false || CheckCanShoot() == false) break;

                    float timeSinceLastShot = Time.time - _lastShotTime;

                    if (timeSinceLastShot < _fireRateInterval)
                    {
                        float waitTime = _fireRateInterval - timeSinceLastShot;

                        await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);

                        if (token.IsCancellationRequested) break;

                        if (_shouldShoot == false || CheckCanShoot() == false) break;
                    }

                    _lastShotTime = Time.time;

                    ShootProjectile();

                    if (_hasInfiniteAmmo == false)
                    {
                        _currentAmmo -= _ammoCostPerShot;

                        ProcessAutoReloading();
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(_fireRateInterval), cancellationToken: token);
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
                    Debug.LogError($"Selected _projectileType {_projectileType} does not have Projectile component");

                    continue;
                }

                if (poolableObject.TryGetComponent(out DamageDealer damageDealer))
                {
                    damageDealer.Configure(_projectileDamage, _projectileAffiliation, _projectileDurability);
                }
                else
                {
                    Debug.LogError($"Selected _projectileType {_projectileType} does not have DamageDealer component");

                    continue;
                }

                poolableObject.transform.SetPositionAndRotation(firepoint.position, firepoint.rotation);

                if (_shouldSetFirepointAsProjectileParent)
                {
                    poolableObject.transform.SetParent(firepoint);
                }
            }
        }

        private void ProcessAutoReloading()
        {
            if (_hasInfiniteAmmo)
            {
                return;
            }

            bool shouldStartReload = false;

            if (_shouldAutoReloadOnNoAmmo && _currentAmmo <= 0)
            {
                shouldStartReload = true;
            }
            else if (_shouldAutoReloadOnLessThanMaxAmmo && _currentAmmo < _maxAmmo)
            {
                shouldStartReload = true;
            }

            bool isCurrentlyReloading = _shouldBlockFireWhileReload ? _weaponState == WeaponState.Reloading
                : _isReloadingInProgress;

            if (shouldStartReload && !isCurrentlyReloading)
            {
                StartReload();
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

                while (token.IsCancellationRequested == false && _currentAmmo < _maxAmmo)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_reloadLength), cancellationToken: token);

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    _currentAmmo += _ammoPerReload;

                    if (_currentAmmo > _maxAmmo)
                    {
                        _currentAmmo = _maxAmmo;
                    }

                    if (_shouldShoot && CheckCanShoot() && _weaponState != WeaponState.Shooting)
                    {
                        TryStartShooting();
                    }
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
            }
        }

        private void DisposeShootingCancellationToken()
        {
            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = null;
        }

        private void DisposeReloadingCancellationToken()
        {
            _reloadCTS?.Cancel();
            _reloadCTS?.Dispose();
            _reloadCTS = null;
        }
    }
}
