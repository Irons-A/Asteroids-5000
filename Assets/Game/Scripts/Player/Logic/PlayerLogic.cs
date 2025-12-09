using Core.Configuration;
using Core.Physics;
using Core.Systems.ObjectPools;
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
        private PlayerPresentation _playerPresentation;
        private Transform _playerTransform;
        private PlayerSettings _playerSettings; 
        private CustomPhysics _playerPhysics;
        private UniversalObjectPool _objectPool;
        private PlayerWeaponSystem _bulletWeaponSystem;
        private PlayerWeaponSystem _laserWeaponSystem;

        [Inject]
        private void Construct(PlayerPresentation playerView, JsonConfigProvider configProvider,
            CustomPhysics playerPhysics, UniversalObjectPool objectPool)
        {
            _playerSettings = configProvider.PlayerSettingsRef;

            _playerPresentation = playerView;
            _playerTransform = playerView.transform;

            _playerPhysics = playerPhysics;
            _playerPhysics.SetMovableObject(playerView);

            _objectPool = objectPool;
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

        public void ShootBullets(bool value)
        {
            _bulletWeaponSystem.SetShouldShoot(value);
        }

        public void ShootLaser(bool value)
        {
            _laserWeaponSystem.SetShouldShoot(value);
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

        private void ConfigureBulletWeaponSystem()
        {

        }

        private void ConfigureLaserWeaponSystem()
        {

        }
    }
}
