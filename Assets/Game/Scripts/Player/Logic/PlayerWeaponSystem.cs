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
        private bool _wasShootingLastFrame = false; // Для отслеживания изменений
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
        private int _maxAmmo; // Установите разумное значение по умолчанию
        private int _currentAmmo;
        private int _ammoCostPerShot;
        private bool _hasInfiniteAmmo;
        private float _reloadLength;
        private int _ammoPerReload;
        private bool _shouldAutoReloadOnLessThanMaxAmmo;
        private bool _shouldAutoReloadOnNoAmmo;
        private bool _shouldDepleteAmmoOnReload;
        private bool _shouldBlockFireWhileReload;

        // Добавим для отладки
        private int _tickCount = 0;

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
            _tickCount++;

            // Проверяем изменения в shouldShoot
            if (_shouldShoot != _wasShootingLastFrame)
            {
                //Debug.Log($"Tick {_tickCount}: shouldShoot changed from {_wasShootingLastFrame} to {_shouldShoot}");

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

            // Обработка автоматической перезарядки
            ProcessAutoReloading();

            // Отладочная информация каждые 60 кадров
            if (_tickCount % 60 == 0)
            {
                //DebugInfo();
            }
        }

        private bool CheckCanShoot()
        {
            // Проверяем наличие патронов
            bool hasEnoughAmmo = _hasInfiniteAmmo || _currentAmmo >= _ammoCostPerShot;

            // Проверяем блокировку перезарядкой
            bool notBlockedByReload = !(_weaponState == WeaponState.Reloading && _shouldBlockFireWhileReload);

            // Также проверяем, что у нас есть firepoints
            bool hasFirepoints = _firePoints != null && _firePoints.Length > 0;

            return hasEnoughAmmo && notBlockedByReload && hasFirepoints;
        }

        private void TryStartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            bool canShoot = CheckCanShoot();
            Debug.Log($"TryStartShooting: canShoot={canShoot}, weaponState={_weaponState}");

            if (canShoot && _weaponState == WeaponState.Idle)
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
            Debug.Log($"shouldShoot: {_shouldShoot}, weaponState: {_weaponState}");
            Debug.Log($"canShoot: {canShoot}");
            Debug.Log($"ammo: {_currentAmmo}/{_maxAmmo}, infinite: {_hasInfiniteAmmo}");
        }

        public void SetShouldShoot(bool value)
        {
            // Добавим проверку на изменение значения
            if (value == _shouldShoot) return;

            //Debug.Log($"SetShouldShoot: {_shouldShoot} -> {value} at time {Time.time}");
            _shouldShoot = value;

            // Немедленная реакция на изменение
            if (_shouldShoot)
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
            if (_weaponState == WeaponState.Reloading ||
                _currentAmmo == _maxAmmo ||
                _hasInfiniteAmmo)
            {
                return;
            }

            Debug.Log("Starting reload...");

            // Останавливаем стрельбу если нужно
            if (_shouldBlockFireWhileReload && _weaponState == WeaponState.Shooting)
            {
                StopShooting();
            }

            _weaponState = WeaponState.Reloading;

            // Отменяем предыдущую перезарядку
            _reloadCTS?.Cancel();
            _reloadCTS?.Dispose();
            _reloadCTS = new CancellationTokenSource();

            ReloadLoop(_reloadCTS.Token).Forget();
        }

        private void ProcessAutoReloading()
        {
            if (_hasInfiniteAmmo) return;

            // Автоперезарядка только в состоянии Idle
            if (_weaponState != WeaponState.Idle) return;

            bool shouldReload = false;

            if (_shouldAutoReloadOnNoAmmo && _currentAmmo <= 0)
            {
                shouldReload = true;
            }
            else if (_shouldAutoReloadOnLessThanMaxAmmo && _currentAmmo < _maxAmmo)
            {
                shouldReload = true;
            }

            if (shouldReload)
            {
                StartReload();
            }
        }

        private void StartShooting()
        {
            if (_weaponState == WeaponState.Shooting) return;

            //Debug.Log($"StartShooting called at time {Time.time}");
            _weaponState = WeaponState.Shooting;

            // Отменяем предыдущую стрельбу
            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = new CancellationTokenSource();

            ShootingLoop(_shootingCTS.Token).Forget();
        }

        private void StopShooting()
        {
            if (_weaponState != WeaponState.Shooting) return;

            //Debug.Log($"StopShooting called at time {Time.time}");
            _weaponState = WeaponState.Idle;

            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = null;
        }

        private async UniTaskVoid ShootingLoop(CancellationToken token)
        {
            //Debug.Log($"ShootingLoop started at time {Time.time}");

            try
            {
                // Первый выстрел немедленно
                if (!token.IsCancellationRequested && _weaponState == WeaponState.Shooting)
                {
                    if (CheckCanShoot())
                    {
                        Debug.Log($"First shot at time {Time.time}");
                        ShootProjectile();
                    }
                    else
                    {
                        Debug.Log("Cannot shoot, stopping shooting loop");
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
                        Debug.Log("Shooting conditions no longer met, stopping");
                        break;
                    }

                    //Debug.Log($"Next shot at time {Time.time}");
                    ShootProjectile();
                }
            }
            catch (OperationCanceledException)
            {
                // Обработка отмены - это нормально
                Debug.Log("Shooting loop cancelled");
            }
            finally
            {
                //Debug.Log($"ShootingLoop ended at time {Time.time}");
                if (_weaponState == WeaponState.Shooting)
                {
                    _weaponState = WeaponState.Idle;
                }

                DebugInfo();
            }
        }

        private void ShootProjectile()
        {
            if (_firePoints == null || _firePoints.Length == 0)
            {
                Debug.LogError("No firepoints attached!");
                return;
            }

            //Debug.Log($"Shooting projectile from {_firePoints.Length} firepoint(s)");

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
                else
                {
                    poolableObject.transform.SetParent(null);
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
                    _currentAmmo = 0;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_reloadLength), cancellationToken: token);

                if (token.IsCancellationRequested)
                {
                    Debug.Log("Reload cancelled during delay");
                    return;
                }

                _currentAmmo += _ammoPerReload;
                _currentAmmo = Mathf.Min(_currentAmmo, _maxAmmo);

                Debug.Log($"Reload completed. Ammo: {_currentAmmo}/{_maxAmmo}");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Reload cancelled");
            }
            finally
            {
                if (_weaponState == WeaponState.Reloading)
                {
                    _weaponState = WeaponState.Idle;
                }
                Debug.Log("ReloadLoop ended");
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

            _reloadCTS?.Cancel();
            _weaponState = WeaponState.Idle;
        }
    }
}
