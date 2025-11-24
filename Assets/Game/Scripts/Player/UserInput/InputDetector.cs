using Core.Configuration;
using Player.Logic;
using Player.UserInput.Strategies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Player.UserInput
{
    public class InputDetector : MonoBehaviour
    {
        private IInputStrategy _currentStrategy;
        private KeyboardMouseInputStrategy _pcStrategy;
        private GamepadInputStrategy _gamepadStrategy;

        private PlayerModel _playerModel;

        private JsonConfigProvider _configProvider;
        private UserInputSettings _inputSettings;

        [Inject]
        private void Construct(KeyboardMouseInputStrategy pcStrategy, GamepadInputStrategy gamepadStrategy,
            PlayerModel playermodel, JsonConfigProvider configProvider)
        {
            _configProvider = configProvider;
            _inputSettings = configProvider.LoadUserInputSettings();

            _pcStrategy = pcStrategy;
            _gamepadStrategy = gamepadStrategy;
            _currentStrategy = pcStrategy;

            _playerModel = playermodel;
        }

        private void Update()
        {
            DefineInputStrategy();

            ProcessUserInput();
        }

        private void DefineInputStrategy()
        {
            if (CheckIfMobileInput())
            {

            }
            else if (CheckIfGamepadInput())
            {
                _currentStrategy = _gamepadStrategy;
            }
            else
            {
                _currentStrategy = _pcStrategy;
            }
        }

        private bool CheckIfMobileInput()
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_WSA
                    return Application.isMobilePlatform || 
                           Input.touchCount > 0 || 
                           Input.touches.Length > 0;
            #else
                        return false;
            #endif
        }

        private bool CheckIfGamepadInput()
        {
            return Input.GetKey(KeyCode.JoystickButton0) || 
                Input.GetKey(KeyCode.JoystickButton1) ||
                Mathf.Abs(Input.GetAxis(_inputSettings.GamepadHorizontalRotationAxis)) > _inputSettings.InputDeadzone || 
                Mathf.Abs(Input.GetAxis(_inputSettings.GamepadShootBulletKey)) > _inputSettings.InputDeadzone ||
                Mathf.Abs(Input.GetAxis(_inputSettings.GamepadShootLaserKey)) > _inputSettings.InputDeadzone;
        }

        private void ProcessUserInput()
        {
            if (_currentStrategy is KeyboardMouseInputStrategy)
            {
                _playerModel.RotatePlayerWithMouse(_currentStrategy.GetRotationInput());
            }
            else
            {
                _playerModel.RotatePlayerTowardsStick(_currentStrategy.GetRotationInput());
            }

            _playerModel.MovePlayer(_currentStrategy.GetPlayerMovementState());

            if (_currentStrategy.IsShootingBullets())
            {
                _playerModel.ShootBullets();
            }

            if (_currentStrategy.IsShootingLaser())
            {
                _playerModel.ShootBullets();
            }

            if (_currentStrategy.IsPausePressed())
            {
                _playerModel.TogglePause();
            }
        }
    }
}
