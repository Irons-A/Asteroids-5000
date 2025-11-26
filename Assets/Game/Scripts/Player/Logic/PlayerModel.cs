using Core.Configuration;
using Core.Physics;
using Player.UserInput;
using Player.View;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerModel : IFixedTickable
    {
        private PlayerView _playerView;
        private Transform _playerTransform;
        private PlayerSettings _playerSettings;
        private CustomPhysics _playerPhysics;

        private JsonConfigProvider _configProvider;

        [Inject]
        private void Construct(PlayerView playerView, JsonConfigProvider configProvider, CustomPhysics playerPhysics)
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

            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.y) * Mathf.Rad2Deg;

            float currentAngle = _playerTransform.eulerAngles.z;

            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
                _playerSettings.RotationSpeed * Time.deltaTime);

            _playerTransform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }
}
