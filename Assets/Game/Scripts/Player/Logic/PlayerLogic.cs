using Core.Configuration;
using Core.Physics;
using Player.Presentation;
using Player.UserInput;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerLogic : IFixedTickable
    {
        private PlayerPresentation _playerView;
        private Transform _playerTransform;
        private PlayerSettings _playerSettings;
        private CustomPhysics _playerPhysics;

        private JsonConfigProvider _configProvider;

        [Inject]
        private void Construct(PlayerPresentation playerView, JsonConfigProvider configProvider, CustomPhysics playerPhysics)
        {
            _configProvider = configProvider;
            _playerSettings = configProvider.PlayerSettingsRef;

            _playerView = playerView;
            _playerTransform = playerView.transform;

            _playerPhysics = playerPhysics;
            _playerPhysics.SetMovableObject(playerView);
        }

        public void FixedTick()
        {
            _playerPhysics.ProcessPhysics();
        }

        public void RotatePlayerWithMouse(Vector2 mouseWorldPosition)
        {
            Vector2 transformVector2 = _playerTransform.position;

            Vector2 direction = (mouseWorldPosition - transformVector2);

            RotatePlayerAtSpeed(direction);
        }

        public void RotatePlayerTowardsStick(Vector2 direction)
        {
            RotatePlayerAtSpeed(direction);
        }

        public void MovePlayer(PlayerMovementState movementState)
        {
            if (movementState == PlayerMovementState.Accelerating)
            {
                _playerPhysics.ApplyAcceleration(_playerSettings.AccelerationSpeed, _playerSettings.MaxSpeed);
            }
            else if (movementState == PlayerMovementState.Decelerating)
            {
                _playerPhysics.ApplyDeceleration(_playerSettings.DecelerationSpeed);
            }
        }

        public void ShootBullets()
        {

        }

        public void ShootLaser()
        {

        }

        public void TogglePause()
        {

        }

        private void RotatePlayerAtSpeed(Vector2 targetDirection)
        {
            targetDirection = targetDirection.normalized;

            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;

            _playerTransform.rotation = Quaternion.RotateTowards(_playerTransform.rotation,
                Quaternion.Euler(0, 0, targetAngle), _playerSettings.RotationSpeed * Time.deltaTime);
        }
    }
}
