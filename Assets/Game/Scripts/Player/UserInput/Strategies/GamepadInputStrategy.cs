using Core.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Player.UserInput.Strategies
{
    public class GamepadInputStrategy : IInputStrategy
    {
        private JsonConfigProvider _configProvider;
        private UserInputSettings _inputSettings;

        private Vector2 _rotation = new Vector2(1, 0);

        [Inject]
        private void Construct(JsonConfigProvider configProvider)
        {
            _configProvider = configProvider;

            _inputSettings = configProvider.LoadUserInputSettings();
        }

        public PlayerMovementState GetPlayerMovementState()
        {
            if (Input.GetKey(_inputSettings.GamepadAccelerationKey))
            {
                return PlayerMovementState.Accelerating;
            }
            else if (Input.GetKey(_inputSettings.GamepadDecelrationKey))
            {
                return PlayerMovementState.Decelerating;
            }
            else
            {
                return PlayerMovementState.Idle;
            }
        }

        public Vector2 GetRotationInput()
        {
            float horizontal = Input.GetAxis(_inputSettings.GamepadHorizontalRotationAxis);
            float vertical = Input.GetAxis(_inputSettings.GamepadVerticalRotationAxis);

            Vector2 inputRotation = new Vector2(horizontal, vertical);

            if (inputRotation.magnitude > _inputSettings.InputDeadzone)
            {
                _rotation = inputRotation;
            }

            return _rotation;
        }

        public bool IsPausePressed()
        {
            return Input.GetKeyDown(_inputSettings.GamepadPauseKey);
        }

        public bool IsShootingBullets()
        {
            return GetIsTriggerPressed(_inputSettings.GamepadShootBulletKey);
        }

        public bool IsShootingLaser()
        {
            return GetIsTriggerPressed(_inputSettings.GamepadShootLaserKey);
        }

        private bool GetIsTriggerPressed(string triggerName)
        {
            float triggerValue = Input.GetAxis(triggerName);

            if (triggerValue > _inputSettings.InputDeadzone)
            {
                return true;
            }

            return false;
        }
    }
}
