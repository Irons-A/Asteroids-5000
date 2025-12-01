using Core.Configuration;
using Cysharp.Threading.Tasks;
using Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Gameplay.Environment.Systems
{
    public class EnemySpawner
    {
        private EnvironmentSettings _environmentSettings;
        //enemy settings
        //enemy factory

        private Bounds _gameFieldBounds;
        private CancellationTokenSource _spawnCTS;
        private int _livingEnemyCount = 0; //Decrease with signal bus events

        [Inject]
        private void Construct(JsonConfigProvider configProvider)
        {
            _environmentSettings = configProvider.EnvironmentSettingsRef;
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

                        // EnemyFactory.Spawn(enemyType);

                        _livingEnemyCount++;

                        Debug.Log($"Enemy spawned at: {spawnPosition}");
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

        private EnemyType EnemyToSpawn()
        {
            float ufoSpawnChance = Mathf.Clamp01(_environmentSettings.UFOSpawnChance);
            float randomEnemyType = Random.Range(0f,1f);

            if (randomEnemyType <= ufoSpawnChance)
            {
                return EnemyType.UFO;
            }
            else
            {
                return EnemyType.BigAsteroid;
            }
        }
    }
}
