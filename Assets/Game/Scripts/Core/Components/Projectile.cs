using Core.Systems.ObjectPools;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Core.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private Transform _transform;
        private PoolableObject _poolableObject;

        private float _speed = 0;
        private bool _delayedDestruction = false;
        private float _destroyAfter = 1f;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDelayedDestructionTaskRunning = false;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            
            if (TryGetComponent(out PoolableObject poolableObject))
            {
                _poolableObject = poolableObject;
            }
        }

        private void OnDestroy()
        {
            CancelCurrentTask();
        }

        private void Update()
        {
            _transform.Translate(Vector3.right * _speed * Time.deltaTime);
        }

        public void Configure(float speed, bool delayedDestruction = false, float destroyAfter = 1)
        {
            _speed = speed;
            _delayedDestruction = delayedDestruction;
            _destroyAfter = destroyAfter;

            if (_delayedDestruction)
            {
                StartDelayedDeactivation();
            }
        }

        private void StartDelayedDeactivation()
        {
            CancelCurrentTask();

            _cancellationTokenSource = new CancellationTokenSource();

            _isDelayedDestructionTaskRunning = true;

            DelayedDeactivationAsync(_cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid DelayedDeactivationAsync(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_destroyAfter), cancellationToken: token);

                if (this == null || !_isDelayedDestructionTaskRunning) return;

                if (_poolableObject == null)
                {
                    Destroy(gameObject);
                }
                else
                {
                    _poolableObject.Despawn();
                }

                _isDelayedDestructionTaskRunning = false;
            }
            catch (OperationCanceledException)
            {
                _isDelayedDestructionTaskRunning = false;

                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error in delayed deactivation: {e.Message}");

                _isDelayedDestructionTaskRunning = false;
            }
        }

        private void CancelCurrentTask()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            _isDelayedDestructionTaskRunning = false;
        }
    }
}
