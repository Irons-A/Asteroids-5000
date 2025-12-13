using Core.Systems.ObjectPools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Systems
{
    public class OutsideOfViewportDestroyer : MonoBehaviour
    {
        [SerializeField] private float _viewportMargin = 0;
        [SerializeField] private float _checkInterval = 0.1f;

        private Camera _mainCamera;
        private Transform _selfTransform;
        private float _nextCheckTime;

        public event Action OnLeftViewport;

        private void Awake()
        {
            _selfTransform = transform;
            _nextCheckTime = Time.time + _checkInterval;
        }

        public void ProcessChecks()
        {
            if (Time.time >= _nextCheckTime)
            {
                CheckViewportVisibility();
                _nextCheckTime = Time.time + _checkInterval;
            }
        }

        private void CheckViewportVisibility()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            Vector3 viewportPos = _mainCamera.WorldToViewportPoint(_selfTransform.position);

            bool isOutsideViewport = viewportPos.x < -_viewportMargin || viewportPos.x > 1 + _viewportMargin ||
                                    viewportPos.y < -_viewportMargin || viewportPos.y > 1 + _viewportMargin;

            if (isOutsideViewport)
            {
                OnLeftViewport?.Invoke();
            }
        }
    }
}
