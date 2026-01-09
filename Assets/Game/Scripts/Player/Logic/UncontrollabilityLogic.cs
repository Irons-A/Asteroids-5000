using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Player.Logic
{
    public class UncontrollabilityLogic : IDisposable
    {
        private CancellationTokenSource _uncontrollabilityCTS;

        private float _uncontrollabilityDuration;
        private bool _isConfigured = false;
        
        public bool IsUncontrollable { get; private set; } = false;

        public void Configure(float uncontrollabilityDuration)
        {
            _uncontrollabilityDuration = uncontrollabilityDuration;
            
            _isConfigured = true;
        }

        public void StartUncontrollabilityPeriod()
        {
            if (IsUncontrollable || _isConfigured == false) return;
            
            StopUncontrollabilityPeriod();
                
            _uncontrollabilityCTS = new CancellationTokenSource();
                
            IsUncontrollable = true;

            UncontrolabilityTask(_uncontrollabilityCTS.Token).Forget();
        }

        public void StopUncontrollabilityPeriod()
        {
            _uncontrollabilityCTS?.Cancel();
            _uncontrollabilityCTS?.Dispose();
            _uncontrollabilityCTS = null;
            IsUncontrollable = false;
        }

        private async UniTaskVoid UncontrolabilityTask(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_uncontrollabilityDuration),
                    cancellationToken: token);

                if (IsUncontrollable == false) return;

                IsUncontrollable = false;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                StopUncontrollabilityPeriod();
            }
        }
        
        public void Dispose()
        {
            StopUncontrollabilityPeriod();
        }
    }
}
