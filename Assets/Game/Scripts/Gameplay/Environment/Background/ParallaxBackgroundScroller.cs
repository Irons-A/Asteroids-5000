using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Environment.Background
{
    public class ParallaxBackgroundScroller : MonoBehaviour
    {
        [Header("Background Layers")]
        [SerializeField] private List<ParallaxBackgroundLayer> _backgroundLayers = new List<ParallaxBackgroundLayer>();

        [Header("Settings")]
        [SerializeField] private bool _autoAssignLayers = true;
        [SerializeField] private float _globalSpeedMultiplier = 0.02f;
        [SerializeField] private bool _followCamera = true;

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
                _backgroundLayers.AddRange(GetComponentsInChildren<ParallaxBackgroundLayer>());
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
            if (_followCamera)
            {
                transform.position = new Vector3(_cameraTransform.position.x, _cameraTransform.position.y,
                    transform.position.z);
            }

            UpdateBackgroundScrolling();
        }

        private void UpdateBackgroundScrolling()
        {
            Vector2 cameraMovement = (Vector2)_cameraTransform.position - _lastCameraPosition;

            for (int i = 0; i < _backgroundLayers.Count; i++)
            {
                ParallaxBackgroundLayer layer = _backgroundLayers[i];

                if (layer == null) continue;

                Vector2 textureOffset = _layerOffsets[i];

                if (layer.UseCameraParallax)
                {
                    textureOffset += new Vector2(cameraMovement.x * layer.ScrollSpeed.x * _globalSpeedMultiplier,
                        cameraMovement.y * layer.ScrollSpeed.y * _globalSpeedMultiplier);
                }
                else
                {
                    //constant scrolling unrelated to the camera
                    textureOffset += layer.ScrollSpeed * _globalSpeedMultiplier * Time.deltaTime;
                }

                textureOffset.x = Mathf.Repeat(textureOffset.x, 1);
                textureOffset.y = Mathf.Repeat(textureOffset.y, 1);

                layer.UpdateTextureOffset(textureOffset);

                _layerOffsets[i] = textureOffset;
            }

            _lastCameraPosition = _cameraTransform.position;
        }
    }
}
