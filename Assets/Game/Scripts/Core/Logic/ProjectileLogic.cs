using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Core.Logic
{
    public class ProjectileLogic : IDisposable
    {
        private float _speed = 0;
        private bool _delayedDestruction = false;
        private float _destroyAfter = 1f;
        
        private Transform _projectileTransform;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDelayedDestructionTaskRunning = false;

        public event Action OnDelayedDestructionCalled;

        public void SetPresentationTransform(Transform transform)
        {
            _projectileTransform = transform;
        }

        public void ConfigureParameters(float speed, bool delayedDestruction = false, float destroyAfter = 1)
        {
            _speed = speed;
            _delayedDestruction = delayedDestruction;
            _destroyAfter = destroyAfter;

            if (_delayedDestruction)
            {
                StartDelayedDeactivation();
            }
        }

        public void MoveProjectile()
        {
            _projectileTransform.Translate(Vector3.right * _speed * Time.deltaTime);
        }
        
        public void CancelDelayedDestruction()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _isDelayedDestructionTaskRunning = false;
        }
        
        private void StartDelayedDeactivation()
        {
            CancelDelayedDestruction();

            _cancellationTokenSource = new CancellationTokenSource();

            _isDelayedDestructionTaskRunning = true;

            DelayedDeactivationTask(_cancellationTokenSource.Token).Forget();
        }

        private async UniTask DelayedDeactivationTask(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_destroyAfter), cancellationToken: token);

                if (_isDelayedDestructionTaskRunning == false) return;
                
                OnDelayedDestructionCalled?.Invoke();

                _isDelayedDestructionTaskRunning = false;
            }
            catch (OperationCanceledException)
            {
                _isDelayedDestructionTaskRunning = false;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error in delayed deactivation: {e.Message}");

                _isDelayedDestructionTaskRunning = false;
            }
        }
        
        public void Dispose()
        {
            CancelDelayedDestruction();
            OnDelayedDestructionCalled = null;
        }
    }
}
