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
        private GameObject _projectilePrefab;
        private Transform[] _firePoints;
        private UniversalObjectPool _objectPool;

        private bool _shouldShoot = false;
        private bool _canShoot = true;
        private bool _isShooting = false;
        private CancellationTokenSource _shootingCTS;

        private float _fireRate;
        private int _maxAmmo;
        private int _currentAmmo;
        private int _projectileDamage;
        private int _projectileSpeed;
        private float _reloadSpeed;
        private int _ammoPerReload;

        [Inject]
        private void Construct()
        {

        }

        public void Tick()
        {
            ProcessWillToShoot();
        }

        public void Initialize()
        {

        }

        public void SetShouldShoot(bool value)
        {
            _shouldShoot = value;
        }

        private void ProcessWillToShoot()
        {
            if (_isShooting)
            {
                if (!_shouldShoot || !_canShoot)
                {
                    StopShooting();
                }
            }

            if (_shouldShoot && _canShoot && !_isShooting)
            {
                StartShooting();
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
                ShootBullet();

                await UniTask.Delay(TimeSpan.FromSeconds(_fireRate),ignoreTimeScale: false,
                    cancellationToken: cancellationToken);
            }
        }

        private void ShootBullet()
        {
            if (_projectilePrefab == null || _firePoints == null) return;

            
        }
    }
}
