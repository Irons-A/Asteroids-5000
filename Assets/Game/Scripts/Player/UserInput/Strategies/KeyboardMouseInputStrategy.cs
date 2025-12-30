using Core.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Player.UserInput.Strategies
{
    public class KeyboardMouseInputStrategy : IInputStrategy
    {
        private JsonConfigProvider _configProvider;
        private UserInputSettings _inputSettings;
        private Camera _mainCamera;

        [Inject]
        private void Construct(JsonConfigProvider configProvider)
        {
            _configProvider = configProvider;

            _inputSettings = configProvider.InputSettingsRef;
        }

        public PlayerMovementState GetPlayerMovementState()
        {
            if (Input.GetKey(_inputSettings.PCAccelerationKey))
            {
                return PlayerMovementState.Accelerating;
            }
            else if (Input.GetKey(_inputSettings.PCDecelerationKey))
            {
                return PlayerMovementState.Decelerating;
            }
            else
            {
                return PlayerMovementState.Idle;
            }
        }

        public void SetCamera()
        {
            _mainCamera = Camera.main;
        }

        public Vector2 GetRotationInput()
        {
            if (_mainCamera == null) return Vector2.zero;
            
            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mouseWorldPosition;

            return direction;
        }

        public bool IsPausePressed()
        {
            return Input.GetKeyDown(_inputSettings.PCPauseKey);
        }

        public bool IsShootingBullets()
        {
            return Input.GetKey(_inputSettings.PCShootBulletKey);
        }

        public bool IsShootingLaser()
        {
            return Input.GetKey(_inputSettings.PCShootLaserKey);
        }
    }
}
