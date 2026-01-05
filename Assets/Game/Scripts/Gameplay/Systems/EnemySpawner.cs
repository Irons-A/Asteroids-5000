using Core.Configuration;
using Cysharp.Threading.Tasks;
using Enemies;
using System;
using System.Threading;
using Core.Signal;
using Core.Systems.ObjectPools;
using Enemies.Signals;
using Gameplay.Signals;
using Player.Presentation;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Gameplay.Environment.Systems
{
    public class EnemySpawner : IInitializable, IDisposable
    {
        private readonly EnvironmentSettings _environmentSettings;
        private readonly PoolAccessProvider _objectPool;
        private readonly Transform _playerTransform;
        private readonly SignalBus _signalBus;

        private Bounds _gameFieldBounds;
        private CancellationTokenSource _spawnCTS;
        
        private int _livingEnemyCount = 0;
        
        public EnemySpawner(JsonConfigProvider configProvider, PoolAccessProvider  objectPool,
            PlayerPresentation playerPresentation, SignalBus signalBus)
        {
            _environmentSettings = configProvider.EnvironmentSettingsRef;
            _objectPool = objectPool;
            _playerTransform = playerPresentation.transform;
            _signalBus = signalBus;
        }

        public void SetGameFieldBounds(Bounds gameFieldBounds)
        {
            _gameFieldBounds = gameFieldBounds;
        }

        public void StartEnemySpawning()
        {
            StopEnemySpawning();

            _spawnCTS = new CancellationTokenSource();

            EnemySpawnLoop(_spawnCTS.Token).Forget();
        }

        public void StopEnemySpawning()
        {
            _spawnCTS?.Cancel();
            _spawnCTS?.Dispose();
            _spawnCTS = null;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<EnemySpawnedSignal>(IncreaseEnemyCount);
            _signalBus.Subscribe<EnemyDestroyedSignal>(DecreaseEnemyCount);
            _signalBus.Subscribe<StartEnemySpawningSignal>(StartEnemySpawning);
            _signalBus.Subscribe<DespawnAllSignal>(ResetEnemyCount);
            _signalBus.Subscribe<StopEnemySpawningSignal>(StopEnemySpawning);
        }

        private void DecreaseEnemyCount()
        {
            _livingEnemyCount--;
        }

        private void IncreaseEnemyCount()
        {
            _livingEnemyCount++;
        }

        private void ResetEnemyCount()
        {
            _livingEnemyCount = 0;
        }

        private async UniTaskVoid EnemySpawnLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_environmentSettings.EnemySpawnRate),
                        ignoreTimeScale: false, cancellationToken: cancellationToken);

                    if (_livingEnemyCount < _environmentSettings.MaxEnemiesOnMap)
                    {
                        Vector3 spawnPosition = GetRandomSpawnPosition();
                        PoolableObjectType enemyType = GetEnemyToSpawn();

                        PoolableObject enemy = _objectPool.GetFromPool(enemyType);
                        
                        enemy.transform.position = spawnPosition;

                        if (enemy.TryGetComponent(out EnemyPresentation presentation))
                        {
                            presentation.SetTargetTransform(_playerTransform);
                            presentation.SetAngle(0, shouldRandomize: false, setAngleToPlayer: true);
                        }
                        
                        _livingEnemyCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in enemy spawning: {e.Message}");
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            int side = Random.Range(0, 4);
            Vector3 spawnPosition = Vector3.zero;

            switch (side)
            {
                case 0: //left
                    spawnPosition.x = _gameFieldBounds.min.x - _environmentSettings.EnemySpawnOffset;
                    spawnPosition.y = Random.Range(_gameFieldBounds.min.y, _gameFieldBounds.max.y);

                    break;

                case 1: //right
                    spawnPosition.x = _gameFieldBounds.max.x + _environmentSettings.EnemySpawnOffset;
                    spawnPosition.y = Random.Range(_gameFieldBounds.min.y, _gameFieldBounds.max.y);

                    break;

                case 2: //up
                    spawnPosition.x = Random.Range(_gameFieldBounds.min.x, _gameFieldBounds.max.x);
                    spawnPosition.y = _gameFieldBounds.max.y + _environmentSettings.EnemySpawnOffset;

                    break;

                case 3: //down
                    spawnPosition.x = Random.Range(_gameFieldBounds.min.x, _gameFieldBounds.max.x);
                    spawnPosition.y = _gameFieldBounds.min.y - _environmentSettings.EnemySpawnOffset;

                    break;
            }

            return spawnPosition;
        }

        private PoolableObjectType GetEnemyToSpawn()
        {
            float ufoSpawnChance = Mathf.Clamp01(_environmentSettings.UFOSpawnChance);
            float randomEnemyType = Random.Range(0f,1f);

            if (randomEnemyType <= ufoSpawnChance)
            {
                return PoolableObjectType.UFO;
            }
            else
            {
                return PoolableObjectType.BigAsteroid;
            }
        }
        
        public void Dispose()
        {
            _signalBus.Unsubscribe<EnemySpawnedSignal>(IncreaseEnemyCount);
            _signalBus.Unsubscribe<EnemyDestroyedSignal>(DecreaseEnemyCount);
            _signalBus.Unsubscribe<StartEnemySpawningSignal>(StartEnemySpawning);
            _signalBus.Unsubscribe<DespawnAllSignal>(ResetEnemyCount);
            _signalBus.Unsubscribe<StopEnemySpawningSignal>(StopEnemySpawning);
        }
    }
}
