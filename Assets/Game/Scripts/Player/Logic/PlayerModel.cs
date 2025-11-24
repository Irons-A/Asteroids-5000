using Core.Configuration;
using Player.UserInput;
using Player.View;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Zenject;

namespace Player.Logic
{
    public class PlayerModel
    {
        private PlayerView _playerView;
        private Transform _playerTransform;
        private PlayerSettings _playerSettings;

        private JsonConfigProvider _configProvider;

        [Inject]
        private void Construct(PlayerView playerView, JsonConfigProvider configProvider)
        {
            _configProvider = configProvider;
            _playerSettings = configProvider.LoadPlayerSettings();

            _playerView = playerView;
            _playerTransform = playerView.transform;
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
