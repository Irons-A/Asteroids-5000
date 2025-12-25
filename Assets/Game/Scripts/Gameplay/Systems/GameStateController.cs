using System;
using System.Collections;
using System.Collections.Generic;
using Core.Signal;
using Core.Signals;
using Gameplay.Signals;
using Player.Presentation;
using Player.Signals;
using Player.UserInput;
using UI.Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Systems
{
    public class GameStateController : MonoBehaviour
    {
        [SerializeField] private Canvas _gameUICanvas;
        [SerializeField] private Canvas _menuCanvas;
        [SerializeField] private Canvas _pauseCanvas;
        [SerializeField] private Canvas _gameOverCanvas;
        [SerializeField] private Canvas _mobileControlsCanvas;
        
        private SignalBus _signalBus;

        private IInputStrategy _currentInputStrategy; //how to set? Input detector is created later. via events?
        
        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<ContinueGameSignal>(ContinueGame);
            _signalBus.Subscribe<ExitGameSignal>(ExitGame);
            _signalBus.Subscribe<GoToMenuSignal>(GoToMenu);
            _signalBus.Subscribe<PauseGameSignal>(TogglePause);
            _signalBus.Subscribe<RestartGameSignal>(RestartGame);
            _signalBus.Subscribe<StartGameSignal>(StartGame);
            _signalBus.Subscribe<EndGameSignal>(DisplayGameOverCanvas); //Go to menu or show game over canvas?
        }

        private void TogglePause()
        {
            if (_pauseCanvas.isActiveAndEnabled)
            {
                ContinueGame();
            }
            else
            {
                _pauseCanvas.gameObject.SetActive(true);
                Time.timeScale = 0;
            }
        }

        private void ContinueGame()
        {
            _pauseCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;
        }

        private void ExitGame()
        {
            
        }

        private void GoToMenu()
        {
            Time.timeScale = 0;
            _pauseCanvas.gameObject.SetActive(false);
            _gameUICanvas.gameObject.SetActive(false);
            //_mobileControlsCanvas.gameObject.SetActive(false);
            _gameOverCanvas.gameObject.SetActive(false);
            _menuCanvas.gameObject.SetActive(true);
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
            
            //if (_currentInputStrategy is MobileInputStrategy)
            //_mobileControlsCanvas.gameObject.SetActive(true);
            
            ResetGame();
        }

        private void DisplayGameOverCanvas()
        {
            _gameUICanvas.gameObject.SetActive(false);
            //_mobileControlsCanvas.gameObject.SetActive(false);
            _gameOverCanvas.gameObject.SetActive(true);
        }

        private void ResetGame()
        {
            _signalBus.TryFire(new StartGameSignal());
            _signalBus.TryFire(new DespawnAllSignal());
            _signalBus.TryFire(new StartEnemySpawningSignal());
            _signalBus.TryFire(new ResetPlayerSignal());
        }
    }
}
