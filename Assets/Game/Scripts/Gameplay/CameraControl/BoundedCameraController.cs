using Gameplay.Environment;
using Player.Presentation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.CameraControl
{
    public class BoundedCameraController : MonoBehaviour
    {
        [SerializeField] private float _followSharpness = 5f;
        [SerializeField] private Vector2 _cameraOffset = Vector2.zero;

        private Camera _camera;
        private Vector3 _currentPosition;
        private Transform _targetTransform;
        private Bounds _gameFieldBounds;

        private Vector2 _cameraExtents;
        private Vector2 _gameFieldSize;
        private float _cameraMinX;
        private float _cameraMaxX;
        private float _cameraMinY;
        private float _cameraMaxY;
        private bool _boundsInitialized = false;

        [Inject]
        private void Construct(PlayerPresentation target, EnvironmentLogic environmentController)
        {
            _targetTransform = target.transform;
            _gameFieldBounds = environmentController.Bounds;
            _gameFieldSize = new Vector2(_gameFieldBounds.size.x, _gameFieldBounds.size.y);
        }

        private void Start()
        {
            _camera = Camera.main;
            _currentPosition = transform.position;

            InitializeCameraBounds();

            if (_targetTransform != null)
            {
                _currentPosition = GetTargetCameraPosition();
                transform.position = _currentPosition;
            }
        }

        private void FixedUpdate()
        {
            if (_targetTransform == null || !_boundsInitialized) return;

            Vector3 targetPosition = GetTargetCameraPosition();

            _currentPosition = Vector3.Lerp(_currentPosition, targetPosition, _followSharpness * Time.fixedDeltaTime);

            transform.position = _currentPosition;
        }
        
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector2.zero, _gameFieldSize);
        }

        private void InitializeCameraBounds()
        {
            if (_gameFieldBounds == null) return;

            _cameraExtents = CalculateCameraExtents();

            _cameraMinX = _gameFieldBounds.min.x + _cameraExtents.x;
            _cameraMaxX = _gameFieldBounds.max.x - _cameraExtents.x;
            _cameraMinY = _gameFieldBounds.min.y + _cameraExtents.y;
            _cameraMaxY = _gameFieldBounds.max.y - _cameraExtents.y;

            // if game field is less than camera viewport
            if (_cameraMaxX < _cameraMinX)
                _cameraMinX = _cameraMaxX = _gameFieldBounds.center.x;
            if (_cameraMaxY < _cameraMinY)
                _cameraMinY = _cameraMaxY = _gameFieldBounds.center.y;

            _boundsInitialized = true;
        }

        private Vector2 CalculateCameraExtents()
        {
            if (_camera.orthographic)
            {
                float height = _camera.orthographicSize;
                float width = height * _camera.aspect;

                return new Vector2(width, height);
            }

            return Vector2.zero;
        }

        private Vector3 GetTargetCameraPosition()
        {
            Vector3 targetPos = _targetTransform.position + (Vector3)_cameraOffset;
            targetPos.z = transform.position.z;

            return ApplyCameraBounds(targetPos);
        }

        private Vector3 ApplyCameraBounds(Vector3 desiredPosition)
        {
            return new Vector3(Mathf.Clamp(desiredPosition.x, _cameraMinX, _cameraMaxX),
                Mathf.Clamp(desiredPosition.y, _cameraMinY, _cameraMaxY), desiredPosition.z);
        }
    }
}
