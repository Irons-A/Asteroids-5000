using Gameplay.Environment;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.CameraControl
{
    public class BoundedCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _followSharpness = 5f;
        [SerializeField] private EnvironmentController _gameFieldBounds;
        [SerializeField] private Vector2 _cameraOffset = Vector2.zero;

        private Camera _camera;
        private Vector3 _currentPosition;
        private Vector3 _targetPosition;
        private bool _positionUpdatedThisFrame = false;

        void Start()
        {
            _camera = Camera.main;
            _currentPosition = transform.position;

            if (_target != null)
            {
                _currentPosition = GetTargetCameraPosition();
                transform.position = _currentPosition;
            }
        }

        void FixedUpdate()
        {
            if (_target == null) return;

            _targetPosition = GetTargetCameraPosition();
            _positionUpdatedThisFrame = true;

            _currentPosition = Vector3.Lerp(
                _currentPosition,
                _targetPosition,
                _followSharpness * Time.fixedDeltaTime
            );
        }

        void LateUpdate()
        {
            if (_positionUpdatedThisFrame)
            {
                transform.position = _currentPosition;
                _positionUpdatedThisFrame = false;
            }
            else
            {
                transform.position = _currentPosition;
            }
        }

        private Vector3 GetTargetCameraPosition()
        {
            Vector3 targetPos = _target.position + (Vector3)_cameraOffset;
            targetPos.z = transform.position.z;
            return ApplyCameraBounds(targetPos);
        }

        private Vector3 ApplyCameraBounds(Vector3 desiredPosition)
        {
            if (_gameFieldBounds == null) return desiredPosition;

            Vector2 cameraExtents = GetCameraExtents();
            Bounds fieldBounds = _gameFieldBounds.Bounds;

            float minX = fieldBounds.min.x + cameraExtents.x;
            float maxX = fieldBounds.max.x - cameraExtents.x;
            float minY = fieldBounds.min.y + cameraExtents.y;
            float maxY = fieldBounds.max.y - cameraExtents.y;

            if (maxX < minX) minX = maxX = fieldBounds.center.x;
            if (maxY < minY) minY = maxY = fieldBounds.center.y;

            return new Vector3(
                Mathf.Clamp(desiredPosition.x, minX, maxX),
                Mathf.Clamp(desiredPosition.y, minY, maxY),
                desiredPosition.z
            );
        }

        private Vector2 GetCameraExtents()
        {
            if (_camera.orthographic)
            {
                float height = _camera.orthographicSize;
                float width = height * _camera.aspect;
                return new Vector2(width, height);
            }
            return Vector2.zero;
        }
    }
}
