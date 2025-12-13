using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerReloadingSubsystem
    {
        public event Action OnReloadStarted;
        public event Action OnReloadCompleted;

        public bool IsReloading { get; private set; }
        public bool ShouldBlockFire => _config?.ShouldBlockFireWhileReload ?? false;

        private WeaponConfig _config;
        private CancellationTokenSource _reloadCTS;

        private AmmoManager _ammoManager;

        public void Configure(WeaponConfig config, AmmoManager ammoManager)
        {
            _config = config;
            _ammoManager = ammoManager;
            _ammoManager.SetInfiniteAmmo(config.HasInfiniteAmmo);
        }

        public void StartReload()
        {
            if (_ammoManager.IsFull || _ammoManager.HasInfiniteAmmo) return;
            if (IsReloading) return;

            CancelReload();

            _reloadCTS = new CancellationTokenSource();
            IsReloading = true;

            OnReloadStarted?.Invoke();

            ReloadLoop(_reloadCTS.Token).Forget();
        }

        public void CancelReload()
        {
            if (!IsReloading) return;

            _reloadCTS?.Cancel();
            _reloadCTS?.Dispose();
            _reloadCTS = null;

            IsReloading = false;
        }

        private async UniTaskVoid ReloadLoop(CancellationToken token)
        {
            try
            {
                if (_config.ShouldDepleteAmmoOnReload)
                {
                    _ammoManager.EmptyAmmo();
                }

                while (!token.IsCancellationRequested && !_ammoManager.IsFull)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_config.ReloadLength),
                        cancellationToken: token);

                    if (token.IsCancellationRequested) return;

                    _ammoManager.AddAmmo(_config.AmmoPerReload);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Reload cancelled");
            }
            finally
            {
                IsReloading = false;
                OnReloadCompleted?.Invoke();
            }
        }

        public void ProcessAutoReloading()
        {
            if (_ammoManager.HasInfiniteAmmo) return;

            bool shouldStartReload = false;

            if (_config.ShouldAutoReloadOnNoAmmo && _ammoManager.IsEmpty)
            {
                shouldStartReload = true;
            }
            else if (_config.ShouldAutoReloadOnLessThanMaxAmmo && !_ammoManager.IsFull)
            {
                shouldStartReload = true;
            }

            if (shouldStartReload && !IsReloading)
            {
                StartReload();
            }
        }

        public void Tick()
        {
            // Обработка тика, если нужна
        }

        public void Dispose()
        {
            CancelReload();
        }
    }
}
