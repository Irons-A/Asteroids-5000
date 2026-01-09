using System;
using System.Diagnostics;
using System.Threading;
using Core.Configuration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Gameplay.Infrastructure
{
    public class BootstrapLoader : MonoBehaviour, IInitializable, IDisposable
    {
        private const string GameSceneName = "Game";
        private const float MinLoadingDelay = 1f;
        
        private JsonConfigProvider _configProvider;
        private CancellationTokenSource _cancellationTokenSource;
        
        [Inject]
        private void Construct(JsonConfigProvider configProvider)
        {
            _configProvider = configProvider;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        
        public void Initialize()
        {
            StartLoadingProcess().Forget();
        }
        
        private async UniTaskVoid StartLoadingProcess()
        {
            try
            {
                await LoadWithTimingControl();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Loading failed: {ex.Message}");
            }
        }
        
        private async UniTask LoadWithTimingControl()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            await WaitForConfigsInitialization();
            
            float elapsedTime = (float)stopwatch.Elapsed.TotalSeconds;
            
            float remainingTime = MinLoadingDelay - elapsedTime;
            
            if (remainingTime > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainingTime), 
                    cancellationToken: _cancellationTokenSource.Token);
            }
            
            await LoadGameSceneAsync();
        }
        
        private async UniTask WaitForConfigsInitialization()
        {
            while (_configProvider.IsInitialized == false && _cancellationTokenSource.IsCancellationRequested == false)
            {
                await UniTask.Delay(100, cancellationToken: _cancellationTokenSource.Token);
            }
        }
        
        private async UniTask LoadGameSceneAsync()
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(GameSceneName, LoadSceneMode.Single);
            
            loadOperation.allowSceneActivation = false;

            while (loadOperation.isDone == false)
            {
                if (loadOperation.progress >= 0.9f)
                {
                    await UniTask.Delay(100, cancellationToken: _cancellationTokenSource.Token);
                    
                    loadOperation.allowSceneActivation = true;
                }
                
                await UniTask.Yield();
            }
        }
        
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
