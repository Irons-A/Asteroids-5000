using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Player.Logic.Weapons
{
    public class PlayerReloadingSubsystem
    {
        private PlayerWeaponConfig _config;
        private CancellationTokenSource _reloadCTS;
        private PlayerAmmoSubsystem _ammoManager;

        public event Action OnReloadStarted;
        public event Action OnReloadCompleted;

        public bool IsReloading { get; private set; } = false;
        public bool ShouldBlockFire => _config.ShouldBlockFireWhileReload;
        public float ReloadProgress { get; private set; } = 1;

        public void Configure(PlayerWeaponConfig config, PlayerAmmoSubsystem ammoManager)
        {
            _config = config;
            _ammoManager = ammoManager;
            _ammoManager.SetInfiniteAmmo(config.HasInfiniteAmmo);
        }

        public void StartReload()
        {
            if (_ammoManager.IsFull || _ammoManager.HasInfiniteAmmo || IsReloading) return;
            
            CancelReload();

            _reloadCTS = new CancellationTokenSource();

            IsReloading = true;

            OnReloadStarted?.Invoke();

            ReloadLoop(_reloadCTS.Token).Forget();
        }

        public void CancelReload()
        {
            if (IsReloading == false) return;

            _reloadCTS?.Cancel();
            _reloadCTS?.Dispose();
            _reloadCTS = null;

            IsReloading = false;
        }

        public void ProcessAutoReloading()
        {
            if (_ammoManager.HasInfiniteAmmo) return;

            bool shouldStartReload = false;

            if (_config.ShouldAutoReloadOnNoAmmo && _ammoManager.IsEmpty)
            {
                shouldStartReload = true;
            }
            else if (_config.ShouldAutoReloadOnLessThanMaxAmmo && _ammoManager.IsFull == false)
            {
                shouldStartReload = true;
            }

            if (shouldStartReload && IsReloading == false)
            {
                StartReload();
            }
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
                    await ReloadProgressTask(token);
            
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
        
        private async UniTask ReloadProgressTask(CancellationToken token)
        {
            float duration = _config.ReloadLength;
            float elapsedTime = 0f;
            ReloadProgress = 0f;
    
            while (elapsedTime < duration && !token.IsCancellationRequested)
            {
                ReloadProgress = Mathf.Clamp01(elapsedTime / duration);
                await UniTask.Yield();
                elapsedTime += Time.deltaTime;
            }
    
            if (!token.IsCancellationRequested)
            {
                ReloadProgress = 1f;
            }
        }
        
        public void Dispose()
        {
            CancelReload();
        }
    }
}
