using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Signals;
using Player.Presentation;
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
        
        private SignalBus _signalBus;
        private PlayerPresentation _presentation;
        
        [Inject]
        private void Construct(PlayerPresentation presentation, SignalBus signalBus)
        {
            _presentation = presentation;
            _signalBus = signalBus;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<ContinueGameSignal>();
            _signalBus.Subscribe<ExitGameSignal>();
            _signalBus.Subscribe<GoToMenuSignal>();
            _signalBus.Subscribe<PauseGameSignal>(); //move to player asmdef?
            _signalBus.Subscribe<RestartGameSignal>();
            _signalBus.Subscribe<StartGameSignal>();
            _signalBus.Subscribe<EndGameSignal>();
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
            _menuCanvas.gameObject.SetActive(true);
        }
        
        private void RestartGame()
        {
            ContinueGame();
            //ResetGame
        }

        private void StartGame()
        {
            _menuCanvas.gameObject.SetActive(false);
            _gameUICanvas.gameObject.SetActive(true);
            //ResetGame
        }

        private void ResetGame()
        {
            //сохранение счёта
            //сброс счёта
            //чистка врагов
            //восстановление здоровья игрока
            //включение презентации игрока
            //перенос презентации игрока в центр поля
        }
    }
}
