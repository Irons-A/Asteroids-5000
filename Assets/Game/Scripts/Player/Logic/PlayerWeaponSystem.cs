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
        private bool _isReadyToShoot = true;
        private float _shotCooldownRemaining = 0f;
        private bool _isReloadingInProgress = false;
        private WeaponState _weaponState = WeaponState.Idle;
        private CancellationTokenSource _shootingCTS;
        private CancellationTokenSource _reloadCTS;

        private bool _wantsToShootButNoAmmo = false;
        private float _lastShotTime = 0f;

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
            // Обработка начала/остановки стрельбы при изменении состояния кнопки
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

            // Если кнопка стрельбы зажата, но мы не стреляем - попытаться начать стрельбу
            // Это сработает, если появились патроны после перезарядки
            if (_shouldShoot && _weaponState != WeaponState.Shooting && CheckCanShoot())
            {
                TryStartShooting();
            }

            ProcessAutoReloading();
        }

        private bool CheckCanShoot()
        {
            // Проверяем наличие патронов
            bool hasEnoughAmmo = _hasInfiniteAmmo || _currentAmmo >= _ammoCostPerShot;

            // Проверяем, блокирует ли перезарядка стрельбу
            // Если shouldBlockFireWhileReload == false, то перезарядка не блокирует
            bool notBlockedByReload = !(_isReloadingInProgress && _shouldBlockFireWhileReload);

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

            // Останавливаем стрельбу только если перезарядка блокирует стрельбу
            if (_shouldBlockFireWhileReload && _weaponState == WeaponState.Shooting)
            {
                StopShooting();
            }

            // Устанавливаем состояние только если перезарядка блокирует стрельбу
            if (_shouldBlockFireWhileReload)
            {
                _weaponState = WeaponState.Reloading;
            }

            // Если перезарядка уже идет, не запускаем новую
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

            // Проверяем, нужно ли начинать перезарядку
            bool shouldStartReload = false;

            if (_shouldAutoReloadOnNoAmmo && _currentAmmo <= 0)
            {
                shouldStartReload = true;
            }
            else if (_shouldAutoReloadOnLessThanMaxAmmo && _currentAmmo < _maxAmmo)
            {
                shouldStartReload = true;
            }

            // Если перезарядка не блокирует стрельбу, используем _isReloadingInProgress
            // Если блокирует - используем WeaponState.Reloading
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
            // Если уже стреляем - выходим
            if (_weaponState == WeaponState.Shooting) return;

            // Если идет перезарядка, которая блокирует стрельбу - выходим
            if (_isReloadingInProgress && _shouldBlockFireWhileReload) return;

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

            // Если хотим сбросить кулдаун при полной остановке стрельбы:
            // _lastShotTime = 0f;

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
                    if (token.IsCancellationRequested) break;

                    if (!_shouldShoot || !CheckCanShoot())
                    {
                        break;
                    }

                    // Проверяем, прошло ли достаточно времени с последнего выстрела
                    float timeSinceLastShot = Time.time - _lastShotTime;
                    if (timeSinceLastShot < _fireRateInterval)
                    {
                        // Ждем оставшееся время
                        float waitTime = _fireRateInterval - timeSinceLastShot;
                        await UniTask.Delay(TimeSpan.FromSeconds(waitTime),
                            cancellationToken: token);

                        if (token.IsCancellationRequested) break;
                        if (!_shouldShoot || !CheckCanShoot()) break;
                    }

                    // Обновляем время последнего выстрела и стреляем
                    _lastShotTime = Time.time;
                    ShootProjectile();

                    if (!_hasInfiniteAmmo)
                    {
                        _currentAmmo -= _ammoCostPerShot;
                        ProcessAutoReloading();
                    }

                    // Ждем до следующего возможного выстрела
                    // (это гарантирует минимальную задержку между выстрелами)
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

                // Цикл перезарядки - продолжаем пока есть патроны для перезарядки
                // и не достигли максимума
                while (!token.IsCancellationRequested && _currentAmmo < _maxAmmo)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_reloadLength),
                        cancellationToken: token);

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    // Добавляем патроны
                    _currentAmmo += _ammoPerReload;

                    // Не превышаем максимум
                    if (_currentAmmo > _maxAmmo)
                    {
                        _currentAmmo = _maxAmmo;
                    }

                    Debug.Log($"Reloaded: {_currentAmmo}/{_maxAmmo}");

                    // Если патроны еще не полные, продолжаем перезарядку
                    // (цикл продолжится автоматически)
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

                // После завершения перезарядки проверяем, нужно ли возобновить стрельбу
                if (_shouldShoot && CheckCanShoot())
                {
                    Debug.Log("Auto-starting shooting after reload");
                    TryStartShooting();
                }

                // Если после перезарядки все еще не достигли максимума и есть условия
                // для автоперезарядки, запускаем новую перезарядку
                if (!token.IsCancellationRequested &&
                    _currentAmmo < _maxAmmo &&
                    (_shouldAutoReloadOnLessThanMaxAmmo ||
                     (_shouldAutoReloadOnNoAmmo && _currentAmmo <= 0)))
                {
                    // Небольшая задержка перед следующей проверкой
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
