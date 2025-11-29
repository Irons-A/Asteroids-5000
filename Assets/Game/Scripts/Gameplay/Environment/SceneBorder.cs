using Core.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class SceneBorder : MonoBehaviour
    {
        [Header("Border Settings")]
        [SerializeField] private float _offsetFromGameFieldBorder = 4f;
        [SerializeField] private float _borderThickness = 2f;
        [SerializeField] private float _borderLengthMultiplier = 1.3f;
        [SerializeField] private float _teleportOffset = 1f;

        [SerializeField] private BorderSide _borderSide;
        private float _sceneWidth;
        private float _sceneHeight;

        private BoxCollider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        public void Initialize(BorderSide borderType, float sceneWidth, float sceneHeight)
        {
            _borderSide = borderType;
            _sceneWidth = sceneWidth;
            _sceneHeight = sceneHeight;

            SetupBorder();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out MovableObject movableObject))
            {
                if (movableObject.ShouldTeleport)
                {
                    TeleportObject(movableObject.transform);
                }
                else
                {
                    movableObject.EnableTeleportation();
                }
            }
        }

        private void SetupBorder()
        {
            Vector2 position = Vector2.zero;
            Vector2 size = Vector2.zero;

            switch (_borderSide)
            {
                case BorderSide.Left:
                    position = new Vector2(-_sceneWidth / 2 - _offsetFromGameFieldBorder, 0); 
                    size = new Vector2(_borderThickness, _sceneHeight * _borderLengthMultiplier);
                    break;
                case BorderSide.Right:
                    position = new Vector2(_sceneWidth / 2 + _offsetFromGameFieldBorder, 0);
                    size = new Vector2(_borderThickness, _sceneHeight * _borderLengthMultiplier);
                    break;
                case BorderSide.Top:
                    position = new Vector2(0, _sceneHeight / 2 + _offsetFromGameFieldBorder);
                    size = new Vector2(_sceneWidth * _borderLengthMultiplier, _borderThickness);
                    break;
                case BorderSide.Bottom:
                    position = new Vector2(0, -_sceneHeight / 2 - _offsetFromGameFieldBorder);
                    size = new Vector2(_sceneWidth * _borderLengthMultiplier, _borderThickness);
                    break;
            }

            transform.position = position;
            transform.localScale = size;
        }

        private void TeleportObject(Transform objectTransform)
        {
            Vector3 newPosition = objectTransform.position;

            switch (_borderSide)
            {
                case BorderSide.Left:
                    newPosition.x = _sceneWidth / 2 + _teleportOffset;
                    break;
                case BorderSide.Right:
                    newPosition.x = -_sceneWidth / 2 - _teleportOffset;
                    break;
                case BorderSide.Top:
                    newPosition.y = -_sceneHeight / 2 - _teleportOffset;
                    break;
                case BorderSide.Bottom:
                    newPosition.y = _sceneHeight / 2 + _teleportOffset;
                    break;
            }

            objectTransform.position = newPosition;
        }
    }
}
