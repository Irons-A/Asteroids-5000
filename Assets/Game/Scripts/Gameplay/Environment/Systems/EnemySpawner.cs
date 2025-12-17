using Core.Configuration;
using Cysharp.Threading.Tasks;
using Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Core.Systems.ObjectPools;
using Enemies.Signals;
using Player.Presentation;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Gameplay.Environment.Systems
{
    public class EnemySpawner : IInitializable, IDisposable
    {
        private EnvironmentSettings _environmentSettings;
        private PoolAccessProvider _objectPool;
        private Transform _playerTransform;
        private SignalBus _signalBus;

        private Bounds _gameFieldBounds;
        private CancellationTokenSource _spawnCTS;
        
        private int _livingEnemyCount = 0; //Decrease with signal bus events

        [Inject]
        private void Construct(JsonConfigProvider configProvider, PoolAccessProvider  objectPool,
            PlayerPresentation playerPresentation, SignalBus signalBus)
        {
            _environmentSettings = configProvider.EnvironmentSettingsRef;
            _objectPool = objectPool;
            _playerTransform = playerPresentation.transform;
            _signalBus = signalBus;
        }

        public void StartEnemySpawning(Bounds bounds)
        {
            _gameFieldBounds = bounds;

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
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<EnemySpawnedSignal>(IncreaseEnemyCount);
            _signalBus.Unsubscribe<EnemyDestroyedSignal>(DecreaseEnemyCount);
        }

        private void DecreaseEnemyCount()
        {
            _livingEnemyCount--;
        }

        private void IncreaseEnemyCount()
        {
            _livingEnemyCount++;
        }

        private async UniTaskVoid EnemySpawnLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_environmentSettings.EnemySpawnRate),
                        ignoreTimeScale: false, cancellationToken: cancellationToken);

                    if (_livingEnemyCount < _environmentSettings.MaxEnemiesOnMap)
                    {
                        Vector3 spawnPosition = GetRandomSpawnPosition();
                        PoolableObjectType enemyType = GetEnemyToSpawn();

                        PoolableObject enemy = _objectPool.GetFromPool(enemyType);
                        
                        enemy.transform.position = spawnPosition;
                        
                        Vector3 direction = _playerTransform.position - enemy.transform.position;
                        
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        enemy.transform.rotation = Quaternion.Euler(0, 0, angle);

                        _livingEnemyCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Enemy spawning cancelled");
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
    }
}
