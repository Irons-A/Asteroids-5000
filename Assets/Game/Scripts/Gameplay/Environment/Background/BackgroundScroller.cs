using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Environment.Background
{
    public class BackgroundScroller : MonoBehaviour
    {
        [Header("Background Layers")]
        [SerializeField] private List<BackgroundLayer> _backgroundLayers = new List<BackgroundLayer>();

        [Header("Settings")]
        [SerializeField] private bool _autoAssignLayers = true;
        [SerializeField] private float _globalSpeedMultiplier = 1f;

        private Transform _cameraTransform;
        private Vector2 _lastCameraPosition;
        private Vector2[] _layerOffsets;

        private void Start()
        {
            _cameraTransform = Camera.main.transform;
            _lastCameraPosition = _cameraTransform.position;

            if (_autoAssignLayers)
            {
                _backgroundLayers.Clear();
                _backgroundLayers.AddRange(GetComponentsInChildren<BackgroundLayer>());
            }

            _layerOffsets = new Vector2[_backgroundLayers.Count];

            for (int i = 0; i < _backgroundLayers.Count; i++)
            {
                if (_backgroundLayers[i] != null)
                {
                    _layerOffsets[i] = _backgroundLayers[i].SavedOffset;
                }
            }
        }

        private void Update()
        {
            UpdateBackgroundScrolling();
        }

        private void UpdateBackgroundScrolling()
        {
            Vector2 cameraMovement = (Vector2)_cameraTransform.position - _lastCameraPosition;

            for (int i = 0; i < _backgroundLayers.Count; i++)
            {
                var layer = _backgroundLayers[i];
                if (layer == null) continue;

                Vector2 textureOffset = _layerOffsets[i];

                if (layer.UseParallax)
                {
                    textureOffset += new Vector2(cameraMovement.x * layer.ScrollSpeed.x * _globalSpeedMultiplier,
                        cameraMovement.y * layer.ScrollSpeed.y * _globalSpeedMultiplier);
                }
                else
                {
                    textureOffset += layer.ScrollSpeed * _globalSpeedMultiplier * Time.deltaTime;
                }

                textureOffset.x = Mathf.Repeat(textureOffset.x, 1);
                textureOffset.y = Mathf.Repeat(textureOffset.y, 1);

                layer.UpdateTextureOffset(textureOffset);
                _layerOffsets[i] = textureOffset;
            }

            _lastCameraPosition = _cameraTransform.position;
        }

        public void SetGlobalSpeedMultiplier(float multiplier)
        {
            _globalSpeedMultiplier = multiplier;
        }

        public void AddLayer(BackgroundLayer layer)
        {
            if (!_backgroundLayers.Contains(layer))
            {
                _backgroundLayers.Add(layer);

                System.Array.Resize(ref _layerOffsets, _backgroundLayers.Count);
                _layerOffsets[_backgroundLayers.Count - 1] = layer.SavedOffset;
            }
        }

        public void RemoveLayer(BackgroundLayer layer)
        {
            if (_backgroundLayers.Contains(layer))
            {
                int index = _backgroundLayers.IndexOf(layer);
                _backgroundLayers.RemoveAt(index);

                var newOffsets = new Vector2[_backgroundLayers.Count];
                for (int i = 0, j = 0; i < _layerOffsets.Length; i++)
                {
                    if (i != index)
                    {
                        newOffsets[j++] = _layerOffsets[i];
                    }
                }
                _layerOffsets = newOffsets;
            }
        }
    }
}
