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

        // Добавляем флаг для отслеживания желания стрелять при пустом магазине
        private bool _wantsToShootButNoAmmo = false;

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

            Debug.Log($"Configured: maxAmmo={_maxAmmo}, autoReloadOnLess={_shouldAutoReloadOnLessThanMaxAmmo}, autoReloadOnNoAmmo={_shouldAutoReloadOnNoAmmo}");
        }

        public void Tick()
        {
            // Обработка стрельбы
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

            // Если хотим стрелять, но не можем из-за нехватки патронов,
            // запоминаем это состояние
            if (_shouldShoot && !CheckCanShoot() && !_hasInfiniteAmmo && _currentAmmo < _ammoCostPerShot)
            {
                _wantsToShootButNoAmmo = true;
            }
            else
            {
                _wantsToShootButNoAmmo = false;
            }

            // Автоматическая перезарядка - работает всегда
            ProcessAutoReloading();

            // Если удерживаем стрельбу и появились патроны, начинаем стрелять
            if (_wantsToShootButNoAmmo && CheckCanShoot() && _weaponState != WeaponState.Shooting)
            {
                TryStartShooting();
            }
        }

        private bool CheckCanShoot()
        {
            bool hasEnoughAmmo = _hasInfiniteAmmo || _currentAmmo >= _ammoCostPerShot;

            // Если перезарядка не блокирует стрельбу, то мы можем стрелять даже во время перезарядки
            bool notBlockedByReload = !(_weaponState == WeaponState.Reloading && _shouldBlockFireWhileReload);

            bool hasFirepoints = _firePoints != null && _firePoints.Length > 0;

            return hasEnoughAmmo && notBlockedByReload && hasFirepoints;
        }

        private void TryStartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            bool canShoot = CheckCanShoot();

            // Можем начать стрельбу из Idle или даже из Reloading, если перезарядка не блокирует стрельбу
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
            bool canShoot = CheckCanShoot();
            //Debug.Log($"=== Debug Info ===");
            //Debug.Log($"shouldShoot: {_shouldShoot}, weaponState: {_weaponState}");
            //Debug.Log($"canShoot: {canShoot}");
            Debug.Log($"ammo: {_currentAmmo}/{_maxAmmo}, infinite: {_hasInfiniteAmmo} shouldShoot: {_shouldShoot}, weaponState: {_weaponState}");
            //Debug.Log($"autoReloadOnLess: {_shouldAutoReloadOnLessThanMaxAmmo}, autoReloadOnNoAmmo: {_shouldAutoReloadOnNoAmmo}");
            //.Log($"=== End Debug ===");
        }

        public void SetShouldShoot(bool value)
        {
            if (value == _shouldShoot) return;

            //Debug.Log($"SetShouldShoot: {_shouldShoot} -> {value}");

            _shouldShoot = value;

            if (_shouldShoot)
            {
                TryStartShooting();
            }
            else
            {
                TryStopShooting();
                _wantsToShootButNoAmmo = false; // Сбрасываем флаг, если отпустили кнопку
            }
        }

        public void StartReload()
        {
            if (_currentAmmo == _maxAmmo || _hasInfiniteAmmo)
            {
                //Debug.Log($"Cannot reload: already full ({_currentAmmo}/{_maxAmmo}) or infinite ammo");
                return;
            }

            //Debug.Log($"StartReload called. Current ammo: {_currentAmmo}/{_maxAmmo}, weaponState: {_weaponState}");

            // Если перезарядка блокирует стрельбу и мы стреляем - останавливаем стрельбу
            if (_shouldBlockFireWhileReload && _weaponState == WeaponState.Shooting)
            {
                StopShooting();
            }

            // Устанавливаем состояние перезарядки

            if (_shouldBlockFireWhileReload == true)
            {
                _weaponState = WeaponState.Reloading;
            }

             //!!!!!!!!!!!!!!!

            _reloadCTS?.Cancel();
            _reloadCTS?.Dispose();
            _reloadCTS = new CancellationTokenSource();

            ReloadLoop(_reloadCTS.Token).Forget();
        }

        private void ProcessAutoReloading()
        {
            if (_hasInfiniteAmmo)
            {
                //Debug.Log("Auto-reload skipped: infinite ammo");
                return;
            }

            // Автоперезарядка должна работать, даже если мы сейчас стреляем (если не блокирует)
            // Проверяем условия для автоматической перезарядки
            bool shouldReload = false;

            if (_shouldAutoReloadOnNoAmmo && _currentAmmo <= 0)
            {
                shouldReload = true;
                Debug.Log("Auto-reload condition: No ammo");
            }
            else if (_shouldAutoReloadOnLessThanMaxAmmo && _currentAmmo < _maxAmmo)
            {
                // Автоперезарядка при неполном магазине
                shouldReload = true;
                //Debug.Log($"Auto-reload condition: Less than max ({_currentAmmo}/{_maxAmmo})");
                Debug.Log("Автоперезарядка при неполном магазине");
            }

            if (shouldReload) 
            {
                // Начинаем перезарядку, если еще не перезаряжаемся
                if (_weaponState != WeaponState.Reloading)
                {
                    Debug.Log("Starting auto-reload..."); //!!!!!!!!!!!!!!!!!!!!!!!!!!
                    StartReload();
                }
                else
                {
                    Debug.Log("Already reloading, skipping auto-reload");
                }
            }
        }

        private void StartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            //Debug.Log($"StartShooting. Current ammo: {_currentAmmo}");

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

        private async UniTaskVoid ShootingLoop(CancellationToken token) //дубляж кода
        {
            //Debug.Log("ShootingLoop started");

            try
            {
                // Первый выстрел делаем немедленно
                if (!token.IsCancellationRequested && _weaponState == WeaponState.Shooting)
                {
                    if (CheckCanShoot())
                    {
                        //Debug.Log($"First shot. Ammo before: {_currentAmmo}");
                        ShootProjectile();

                        if (!_hasInfiniteAmmo)
                        {
                            _currentAmmo -= _ammoCostPerShot;
                            _currentAmmo = Mathf.Max(0, _currentAmmo);
                            //Debug.Log($"Ammo after first shot: {_currentAmmo}");

                            // Проверяем автоперезарядку после выстрела
                            await UniTask.NextFrame(token); // Даем кадр на обработку
                            ProcessAutoReloading();
                        }
                    }
                    else
                    {
                        Debug.Log("Cannot shoot first shot, stopping");
                        StopShooting();
                        return;
                    }
                }

                // Цикл для последующих выстрелов
                while (!token.IsCancellationRequested && _weaponState == WeaponState.Shooting)
                {
                    // Ждем перед следующим выстрелом
                    await UniTask.Delay(TimeSpan.FromSeconds(_fireRateInterval),
                        cancellationToken: token);

                    if (token.IsCancellationRequested) break;

                    // Проверяем условия перед каждым выстрелом
                    if (!_shouldShoot || !CheckCanShoot())
                    {
                        //Debug.Log($"Stopping shooting: shouldShoot={_shouldShoot}, canShoot={CheckCanShoot()}");
                        break;
                    }

                    //Debug.Log($"Next shot. Ammo before: {_currentAmmo}");
                    ShootProjectile();

                    if (!_hasInfiniteAmmo)
                    {
                        _currentAmmo -= _ammoCostPerShot;
                        //_currentAmmo = Mathf.Max(0, _currentAmmo);
                        //Debug.Log($"Ammo after: {_currentAmmo}");

                        // Проверяем автоперезарядку после каждого выстрела
                        ProcessAutoReloading();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Shooting loop cancelled");
            }
            finally
            {
                Debug.Log("ShootingLoop ended");
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

                // Активация объекта
                if (!poolableObject.gameObject.activeSelf)
                {
                    poolableObject.gameObject.SetActive(true);
                }
            }
        }

        private async UniTaskVoid ReloadLoop(CancellationToken token)
        {
            Debug.Log("ReloadLoop started");

            try
            {
                if (_shouldDepleteAmmoOnReload)
                {
                    //Debug.Log($"Depleting ammo on reload. Was: {_currentAmmo}");
                    _currentAmmo = 0;
                }

                //Debug.Log($"Waiting {_reloadLength} seconds for reload...");
                await UniTask.Delay(TimeSpan.FromSeconds(_reloadLength), cancellationToken: token);

                if (token.IsCancellationRequested)
                {
                    Debug.Log("Reload cancelled during delay");
                    return;
                }

                _currentAmmo += _ammoPerReload;
                _currentAmmo = Mathf.Min(_currentAmmo, _maxAmmo);

                //Debug.Log($"Reload completed. Ammo: {_currentAmmo}/{_maxAmmo}");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Reload cancelled");
            }
            finally
            {
                // После завершения перезарядки возвращаемся в Idle
                // Но если мы стреляли во время перезарядки (и она не блокировала стрельбу),
                // то остаемся в состоянии Shooting
                if (_weaponState == WeaponState.Reloading)
                {
                    _weaponState = WeaponState.Idle;

                    // Проверяем, не нужно ли начать стрельбу после перезарядки
                }

                if (_shouldShoot && CheckCanShoot())
                {
                    Debug.Log("Auto-starting shooting after reload");
                    TryStartShooting();
                }

                //Debug.Log("ReloadLoop ended");
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
            if (_weaponState != WeaponState.Reloading) return;

            Debug.Log("CancelReload");
            _reloadCTS?.Cancel();

            // После отмены перезарядки проверяем, не нужно ли начать стрельбу
            if (_shouldShoot && CheckCanShoot())
            {
                _weaponState = WeaponState.Shooting;
                StartShooting();
            }
            else
            {
                _weaponState = WeaponState.Idle;
            }
        }
    }
}
