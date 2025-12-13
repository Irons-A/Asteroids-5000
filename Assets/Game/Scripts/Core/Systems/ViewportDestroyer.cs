using Core.Systems.ObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Systems
{
    public class ViewportDestroyer : ITickable, IInitializable
    {
        private float _viewportMargin = 0.2f;
        private float _checkInterval = 0.1f;

        private Camera _mainCamera;
        private Transform _selfTransform;
        private float _nextCheckTime;
        private bool _isActive = false;

        public event Action OnLeftViewport;

        public void Initialize()
        {
            _mainCamera = Camera.main;
            _nextCheckTime = Time.time + _checkInterval;
        }

        public void Configure(Transform transform, float viewportMargin, float checkInterval)
        {
            _selfTransform = transform;
            _viewportMargin = viewportMargin;
            _checkInterval = checkInterval;
        }

        public void Tick()
        {
            Debug.Log($"_isActive {_isActive}");

            if (_isActive == false) return;

            Debug.Log("ViewportDestroyer tick");

            if (Time.time >= _nextCheckTime)
            {
                Debug.Log("ViewportDestroyer CheckViewportVisibility");

                CheckViewportVisibility();

                _nextCheckTime = Time.time + _checkInterval;
            }
        }

        public void SetIsActive(bool value)
        {
            _isActive = value;

            Debug.Log($"_isActive IS SET TO {_isActive}");
        }

        private void CheckViewportVisibility()
        {
            Debug.Log("Main camera null");

            if (_mainCamera == null) return;

            Vector3 viewportPos = _mainCamera.WorldToViewportPoint(_selfTransform.position);

            bool isOutsideViewport = viewportPos.x < -_viewportMargin || viewportPos.x > 1 + _viewportMargin ||
                                    viewportPos.y < -_viewportMargin || viewportPos.y > 1 + _viewportMargin ||
                                    viewportPos.z <= 0;

            if (isOutsideViewport)
            {
                Debug.Log("OnLeftViewport");
                OnLeftViewport?.Invoke();
            }
        }
    }
}
