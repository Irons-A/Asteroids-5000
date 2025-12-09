using Core.Components;
using Core.Projectiles;
using Core.Systems.ObjectPools;
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
        private bool _canShoot = false;
        private bool _isShooting = false;
        private bool _isReloading = false;
        private CancellationTokenSource _shootingCTS;
        private CancellationTokenSource _reloadCTS;

        private float _projectileSpeed;
        private bool _projectileDelayedDestruction;
        private float _destroyProjectileAfter;
        private int _projectileDamage;
        private DamagerAffiliation _projectileAffiliation;
        private DamagerDurability _projectileDurability;

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

        public void Configure(PoolableObjectType projectileType, Transform[] firepoints, float projectileSpeed, bool projectileDelayedDestruction, float destroyProjectileAfter,
            int projectileDamage, DamagerAffiliation projectileAffiliation, DamagerDurability projectileDurability,
            float fireRateInterval, int maxAmmo, int ammoCostPerShot, bool hasInfiniteAmmo, float reloadLength,
            int ammoPerReload, bool shouldAutoReloadOnLessThanMaxAmmo, bool shouldAutoReloadOnNoAmmo,
            bool shouldDepleteAmmoOnReload, bool shouldBlockFireWhileReaload)
        {
            _projectileType = projectileType;
            _firePoints = firepoints;
            _projectileSpeed = projectileSpeed;
            _projectileDelayedDestruction = projectileDelayedDestruction;
            _destroyProjectileAfter = destroyProjectileAfter;
            _projectileDamage = projectileDamage;
            _projectileAffiliation = projectileAffiliation;
            _projectileDurability = projectileDurability;
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
            ProcessAbilityToSHoot();

            ProcessWillToShoot();

            ProcessAutoReloading();
        }

        public void SetShouldShoot(bool value)
        {
            _shouldShoot = value;
        }

        public void StartReload()
        {
            if (_isReloading || _currentAmmo == _maxAmmo || _hasInfiniteAmmo) return;

            if (_shouldBlockFireWhileReload)
            {
                StopShooting();
            }

            _reloadCTS = new CancellationTokenSource();

            ReloadLoop(_reloadCTS.Token).Forget();
        }

        public void CancelReload()
        {
            _isReloading = false;
            _reloadCTS?.Cancel();
            _reloadCTS?.Dispose();
            _reloadCTS = null;
        }

        private void ProcessAbilityToSHoot()
        {
            if (_currentAmmo < _ammoCostPerShot && _hasInfiniteAmmo == false)
            {
                _canShoot = false;
            }
            else if (_isReloading && _shouldBlockFireWhileReload)
            {
                _canShoot = false;
            }
            else
            {
                _canShoot = true;
            }

            if (_isShooting)
            {
                if (_shouldShoot == false || _canShoot == false)
                {
                    StopShooting();
                }
            }
        }

        private void ProcessWillToShoot()
        {
            if (_shouldShoot && _canShoot && _isShooting == false)
            {
                StartShooting();
            }
        }

        private void ProcessAutoReloading()
        {
            if (_hasInfiniteAmmo == false && _currentAmmo <= _maxAmmo && _shouldAutoReloadOnLessThanMaxAmmo)
            {
                StartReload();
            }
        }

        private void StartShooting()
        {
            if (_isShooting) return;

            _isShooting = true;

            _shootingCTS = new CancellationTokenSource();

            ShootingLoop(_shootingCTS.Token).Forget();
        }

        private void StopShooting()
        {
            _isShooting = false;
            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = null;
        }

        private async UniTaskVoid ShootingLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isShooting)
            {
                if (_canShoot)
                {
                    ShootProjectile();

                    if (_hasInfiniteAmmo == false)
                    {
                        _currentAmmo -= _ammoCostPerShot;
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_fireRateInterval),ignoreTimeScale: false,
                    cancellationToken: cancellationToken);

                if (_currentAmmo <= _ammoCostPerShot)
                {
                    if (_shouldAutoReloadOnNoAmmo)
                    {
                        StartReload();
                    }

                    break;
                }
            }
        }

        private void ShootProjectile()
        {
            if (_firePoints == null)
            {
                Debug.Log("No firepoints attached.");

                return;
            }

            foreach (Transform firepoint in _firePoints)
            {
                PoolableObject poolableObject = _objectPool.GetFromPool(_projectileType);

                poolableObject.TryGetComponent(out Projectile projectile);

                if (projectile == null)
                {
                    throw new Exception("Selected _projectileType does not have Projectile Component");
                }
                else
                {
                    projectile.Configure(_projectileSpeed, _projectileDelayedDestruction, _destroyProjectileAfter);
                }

                poolableObject.TryGetComponent(out DamageDealer damageDealer);

                if (damageDealer == null)
                {
                    throw new Exception("Selected _projectileType does not have DamageDelealer Component");
                }
                else
                {
                    damageDealer.Configure(_projectileDamage, _projectileAffiliation, _projectileDurability);
                }
            }
        }

        private async UniTaskVoid ReloadLoop(CancellationToken cancellationToken)
        {
            try
            {
                _isReloading = true;

                if (_shouldDepleteAmmoOnReload)
                {
                    _currentAmmo = 0;
                }

                Debug.Log("reloading");

                await UniTask.Delay(TimeSpan.FromSeconds(_reloadLength), ignoreTimeScale: false,
                    cancellationToken: cancellationToken);

                _currentAmmo += _ammoPerReload;
                _currentAmmo = Math.Min(_currentAmmo, _maxAmmo);

                Debug.Log($"Reloaded, ammo: {_currentAmmo}");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("ReloadCanceled");
            }
            finally
            {
                _isReloading = false;
                _reloadCTS?.Dispose();
                _reloadCTS = null;
            }
        }
    }
}
