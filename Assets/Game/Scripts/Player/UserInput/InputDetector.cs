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
        private void Construct(JsonConfigProvider configProvider, KeyboardMouseInputStrategy pcStrategy,
            GamepadInputStrategy gamepadStrategy, PlayerModel playermodel)
        {
            _configProvider = configProvider;
            _inputSettings = _configProvider.InputSettingsRef;

            _pcStrategy = pcStrategy;
            _gamepadStrategy = gamepadStrategy;
            _currentStrategy = pcStrategy;

            _playerModel = playermodel;
        }

        private void Update()
        {
            DefineInputStrategy();

            ProcessUserInput();

            Debug.Log(_currentStrategy);
        }

        private void DefineInputStrategy()
        {
            bool gamepadInput = CheckIfGamepadConnected() && CheckIfGamepadInput();
            bool pcInput = CheckIfPCInput();

            if (CheckIfMobileInput())
            {

            }
            else if (pcInput)
            {
                _currentStrategy = _pcStrategy;
            }
            else if (gamepadInput)
            {
                _currentStrategy = _gamepadStrategy;
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

        private bool CheckIfPCInput()
        {
            bool hasKeyInput = Input.anyKeyDown;

            bool hasMouseMovement = Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f ||
                                   Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f;

            bool hasMouseClick = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);

            return hasKeyInput || hasMouseMovement || hasMouseClick;
        }

        private bool CheckIfGamepadInput()
        {
            return Input.GetKey(KeyCode.JoystickButton0) ||
                   Input.GetKey(KeyCode.JoystickButton1) ||
                   Input.GetKey(KeyCode.JoystickButton2) ||
                   Input.GetKey(KeyCode.JoystickButton3) ||
                   Input.GetKey(KeyCode.JoystickButton4) ||
                   Input.GetKey(KeyCode.JoystickButton5) ||
                   Input.GetKey(KeyCode.JoystickButton6) ||
                   Input.GetKey(KeyCode.JoystickButton7);
        }

        private bool CheckIfGamepadConnected()
        {
            string[] joystickNames = Input.GetJoystickNames();

            foreach (string joystickName in joystickNames)
            {
                if (!string.IsNullOrEmpty(joystickName) && joystickName.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessUserInput()
        {
            if (_playerModel == null) return;

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
