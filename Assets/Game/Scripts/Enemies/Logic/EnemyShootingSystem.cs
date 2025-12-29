using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Codice.Client.Common;
using Core.Components;
using Core.Configuration;
using Core.Systems;
using Core.Systems.ObjectPools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Enemies.Logic
{
    public class EnemyShootingSystem : IDisposable
    {
        private readonly PoolAccessProvider _objectPool;
        
        private Transform[] _firepoints;
        private PoolableObjectType _projectile;
        private Transform _targetTransform;
        private Transform _selfTransform;
        private EnemyPresentation _selfPresentation;
        private float _shotInterval;
        private int _damage;
        private float _projectileSpeed;
        private bool _isConfigured = false;

        private CancellationTokenSource _shootingCTS;
        private bool _isShooting = false;
        
        public EnemyShootingSystem(PoolAccessProvider objectPool)
        {
            _objectPool = objectPool;
        }

        public void Configure(int damage, float shotInterval, float projectileSpeed, EnemyPresentation selfPresentation,
            PoolableObjectType projectile, Transform[] firepoints)
        {
            _damage = damage;
            _shotInterval = shotInterval;
            _projectileSpeed = projectileSpeed;
            _selfPresentation = selfPresentation;
            _selfTransform = selfPresentation.transform;
            _projectile = projectile;
            _firepoints = firepoints;
            
            _isConfigured = true;
        }

        public void SetTarget(Transform targetTransform)
        {
            _targetTransform = targetTransform;
        }

        public void TryStartShooting()
        {
            if (_isConfigured == false || _isShooting || _targetTransform == null) return;
            
            DisposeShootingCTS();
            
            _shootingCTS = new CancellationTokenSource();

            _isShooting = true;
            
            ShootingTask(_shootingCTS.Token).Forget();
        }

        public void StopShooting()
        {
            DisposeShootingCTS();
        }

        private void DisposeShootingCTS()
        {
            _shootingCTS?.Cancel();
            _shootingCTS?.Dispose();
            _shootingCTS = null;
            _isShooting = false;
        }

        private async UniTaskVoid ShootingTask(CancellationToken token)
        {
            try
            {
                while (token.IsCancellationRequested == false && _selfPresentation.isActiveAndEnabled)
                {
                    ShootProjectile();
                    
                    await UniTask.Delay(TimeSpan.FromSeconds(_shotInterval), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                StopShooting();
            }
        }
        
        private void ShootProjectile()
        {
            if (_firepoints == null || _firepoints.Length == 0)
            {
                Debug.LogError("No firepoints attached on enemy!");

                return;
            }

            foreach (Transform firepoint in _firepoints)
            {
                if (firepoint == null)
                {
                    Debug.LogError("Null firepoint found on enemy!");

                    continue;
                }

                PoolableObject poolableObject = _objectPool.GetFromPool(_projectile);

                if (poolableObject == null)
                {
                    Debug.LogError($"Failed to get projectile from pool: {_projectile}");

                    continue;
                }

                ConfigureProjectile(poolableObject, firepoint);
            }
        }
        
        private void ConfigureProjectile(PoolableObject poolableObject, Transform firepoint)
        {
            if (poolableObject.TryGetComponent(out Projectile projectile))
            {
                projectile.Configure(_projectileSpeed, false, 0);
            }
            else
            {
                Debug.LogError("Projectile does not have Projectile component!");
            }

            if (poolableObject.TryGetComponent(out CollisionHandler collisionHandler))
            {
                collisionHandler.Configure(_damage, EntityAffiliation.Enemy,
                    EntityDurability.Fragile);
            }
            else
            {
                Debug.LogError("Projectile does not have CollisionHandler component!");
            }

            projectile.transform.position = firepoint.position;
            
            Vector3 direction = _targetTransform.position - _selfTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void Dispose()
        {
            StopShooting();
        }
    }
}
