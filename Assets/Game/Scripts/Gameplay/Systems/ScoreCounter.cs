using System;
using System.Collections;
using System.Collections.Generic;
using Core.Configuration;
using Core.Signals;
using Enemies;
using Enemies.Signals;
using UI;
using UI.Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Systems
{
    public class ScoreCounter : IInitializable, IDisposable
    {
        private PlayerUIModel _playerUIModel;
        private JsonConfigProvider _jsonConfigProvider;
        private SignalBus _signalBus;

        private EnemySettings _enemySettings;
        private Dictionary<EnemyType, int> _enemyRewards = new()
        {
            { EnemyType.BigAsteroid, 0},
            { EnemyType.SmallAsteroid, 0},
            { EnemyType.UFO, 0}
        };
        
        private int _currentScore = 0;

        [Inject]
        private void Construct(PlayerUIModel playerUIModel, JsonConfigProvider jsonConfigProvider, SignalBus signalBus)
        {
            _playerUIModel = playerUIModel;
            
            _jsonConfigProvider = jsonConfigProvider;
            _enemySettings = _jsonConfigProvider.EnemySettingsRef;
            
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<EnemyDestroyedSignal>(AddScore);
            _signalBus.Subscribe<EndGameSignal>(TrySavingScore);
            _signalBus.Subscribe<StartGameSignal>(ResetScore);
            
            SetRewards();
        }

        private void SetRewards()
        {
            _enemyRewards[EnemyType.BigAsteroid] = _enemySettings.BigAsteroidReward;
            _enemyRewards[EnemyType.SmallAsteroid] = _enemySettings.SmallAsteroidReward;
            _enemyRewards[EnemyType.UFO] = _enemySettings.UFOReward;
        }

        private void AddScore(EnemyDestroyedSignal signal)
        {
            _currentScore += _enemyRewards[signal.Type];
            _playerUIModel.SetScore(_currentScore);
        }

        private void TrySavingScore()
        {
            
        }

        private void ResetScore()
        {
            _currentScore = 0;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<EnemyDestroyedSignal>(AddScore);
            _signalBus.Unsubscribe<EndGameSignal>(TrySavingScore);
            _signalBus.Unsubscribe<StartGameSignal>(ResetScore);
        }
    }
}
