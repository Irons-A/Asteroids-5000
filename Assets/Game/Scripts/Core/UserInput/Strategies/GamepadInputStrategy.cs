using Core.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.UserInput.Strategies
{
    public class GamepadInputStrategy : IInputStrategy
    {
        private JsonConfigProvider _configProvider;
        private PlayerInputSettings _inputSettings;

        private Vector2 _rotation = Vector2.zero;

        [Inject]
        private void Construct(JsonConfigProvider configProvider)
        {
            _configProvider = configProvider;

            _inputSettings = configProvider.LoadPlayerInputSettings();
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

            if (inputRotation.magnitude > _inputSettings.RotationStickDeadzone)
            {
                _rotation = inputRotation;
            }

            return _rotation;
        }

        public bool IsPausePressed()
        {
            return Input.GetKey(_inputSettings.GamepadPauseKey);
        }

        public bool IsShootingBullets()
        {
            return Input.GetKey(_inputSettings.GamepadShootBulletKey);
        }

        public bool IsShootingLaser()
        {
            return Input.GetKey(_inputSettings.GamepadShootLaserKey);
        }
    }
}
