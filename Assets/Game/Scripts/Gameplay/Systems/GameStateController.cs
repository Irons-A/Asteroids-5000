using System;
using System.Collections;
using System.Collections.Generic;
using Core.Configuration;
using Core.Saves;
using Core.Signal;
using Core.Signals;
using Gameplay.Signals;
using Player.Presentation;
using Player.Signals;
using Player.UserInput;
using Player.UserInput.Strategies;
using UI;
using UI.Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Systems
{
    public class GameStateController : MonoBehaviour
    {
        [Header("Canvases")]
        [SerializeField] private Canvas _gameUICanvas;
        [SerializeField] private Canvas _menuCanvas;
        [SerializeField] private Canvas _pauseCanvas;
        [SerializeField] private Canvas _gameOverCanvas;
        [SerializeField] private Canvas _mobileControlsCanvas;
        
        [Header("ScoreDisplayer")]
        [SerializeField] private ScoreDisplayer _scoreDisplayer;
        
        private SaveSystem _saveSystem;
        private SignalBus _signalBus;
        private ScoreCounter _scoreCounter;
        private InputDetector _inputDetector;
        
        private GameState _currentGameState = GameState.Menu;
        
        [Inject]
        private void Construct(SaveSystem saveSystem, SignalBus signalBus, ScoreCounter scoreCounter,
            InputDetector inputDetector)
        {
            _saveSystem = saveSystem;
            _signalBus = signalBus;
            _scoreCounter = scoreCounter;
            _inputDetector = inputDetector;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<ContinueGameSignal>(ContinueGame);
            _signalBus.Subscribe<ExitGameSignal>(ExitGame);
            _signalBus.Subscribe<GoToMenuSignal>(GoToMenu);
            _signalBus.Subscribe<PauseGameSignal>(TogglePause);
            _signalBus.Subscribe<RestartGameSignal>(RestartGame);
            _signalBus.Subscribe<StartGameSignal>(StartGame);
            _signalBus.Subscribe<EndGameSignal>(DisplayGameOverCanvas);
            
            _scoreDisplayer.DisplayHighScore(_saveSystem.HighScore);
        }

        private void TogglePause()
        {
            if (_pauseCanvas.isActiveAndEnabled == false && _currentGameState == GameState.Game)
            {
                _pauseCanvas.gameObject.SetActive(true);
                Time.timeScale = 0;
                
                _currentGameState = GameState.Pause;
            }
            else
            {
                ContinueGame();
            }
        }

        private void ContinueGame()
        {
            if (_currentGameState == GameState.Pause)
            {
                _pauseCanvas.gameObject.SetActive(false);
            
                Time.timeScale = 1;
                _currentGameState = GameState.Game;
            }
        }

        private void ExitGame()
        {
            Application.Quit();
        }

        private void GoToMenu()
        {
            Time.timeScale = 1;

            StopGame();
            
            _pauseCanvas.gameObject.SetActive(false);
            _gameUICanvas.gameObject.SetActive(false);
            _mobileControlsCanvas.gameObject.SetActive(false);
            _gameOverCanvas.gameObject.SetActive(false);
            _menuCanvas.gameObject.SetActive(true);
            
            _saveSystem.TryUpdatingHighScore(_scoreCounter.CurrentScore);
            _scoreDisplayer.DisplayHighScore(_saveSystem.HighScore);
        }
        
        private void RestartGame()
        {
            ContinueGame();
            ResetGame();
        }

        private void StartGame()
        {
            _menuCanvas.gameObject.SetActive(false);
            _gameUICanvas.gameObject.SetActive(true);

            if (_inputDetector.CurrentStrategy is MobileInputStrategy)
            {
                _mobileControlsCanvas.gameObject.SetActive(true);
            }
            
            ResetGame();
        }

        private void DisplayGameOverCanvas()
        {
            _gameUICanvas.gameObject.SetActive(false);
            _mobileControlsCanvas.gameObject.SetActive(false);
            _gameOverCanvas.gameObject.SetActive(true);
            
            _currentGameState = GameState.GameOver;

            _saveSystem.TryUpdatingHighScore(_scoreCounter.CurrentScore);
            _scoreDisplayer.RefreshScores((_saveSystem).HighScore, _scoreCounter.CurrentScore);
        }

        private void ResetGame()
        {
            _signalBus.TryFire(new ResetScoreSignal());
            _signalBus.TryFire(new DespawnAllSignal());
            _signalBus.TryFire(new StartEnemySpawningSignal());
            _signalBus.TryFire(new ResetPlayerSignal());
            
            _currentGameState = GameState.Game;
        }

        private void StopGame()
        {
            _signalBus.TryFire(new DisablePlayerSignal());
            _signalBus.TryFire(new DespawnAllSignal());
            _signalBus.TryFire(new StopEnemySpawningSignal());
            
            _currentGameState = GameState.Menu;
        }
    }
}
