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
        private Projectile _projectilePrefab;
        private Transform[] _firePoints;
        private UniversalObjectPool _objectPool;

        private bool _shouldShoot = false;
        private bool _canShoot = true;
        private bool _isShooting = false;
        private bool _isReloading = false;
        private CancellationTokenSource _shootingCTS;
        private CancellationTokenSource _reloadCTS;

        private float _fireRate;
        private int _maxAmmo;
        private int _ammoCostPerShot;
        private bool _hasInfiniteAmmo;
        private int _currentAmmo;
        private int _projectileDamage;
        private int _projectileSpeed;
        private float _reloadLength;
        private int _ammoPerReload;
        private bool _shouldAutoReloadOnLessThanMaxAmmo;
        private bool _shouldAutoReloadOnNoAmmo;
        private bool _shouldDepleteAmmoOnReload;
        private bool _shouldBlockFireWhileReload;

        [Inject]
        private void Construct()
        {


        }
        public void Initialize()
        {

        }

        public void Tick()
        {
            ProcessAbilityToSHoot();

            ProcessWillToShoot();

            ProcessAutoReloading();
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

                await UniTask.Delay(TimeSpan.FromSeconds(_fireRate),ignoreTimeScale: false,
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
            if (_projectilePrefab == null || _firePoints == null) return;

            foreach (Transform firepoint in _firePoints)
            {

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
    }
}
