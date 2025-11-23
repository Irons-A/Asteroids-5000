using Core.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.UserInput.Strategies
{
    public class KeyboardMouseInputStrategy : IInputStrategy
    {
        private JsonConfigProvider _configProvider;
        private PlayerInputSettings _inputSettings;
        private Camera _mainCamera;

        [Inject]
        private void Construct(JsonConfigProvider configProvider)
        {
            _configProvider = configProvider;
            _mainCamera = Camera.main;

            _inputSettings = configProvider.LoadPlayerInputSettings();
        }

        public PlayerMovementState GetPlayerMovementState()
        {
            if (Input.GetKey(_inputSettings.PCAccelerationKey))
            {
                return PlayerMovementState.Accelerating;
            }
            else if (Input.GetKey(_inputSettings.PCDecelrationKey))
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
            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mouseWorldPosition.normalized;

            return direction;
        }

        public bool IsPausePressed()
        {
            return Input.GetKey(_inputSettings.PCPauseKey);
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
