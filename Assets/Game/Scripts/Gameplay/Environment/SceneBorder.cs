using Core.Physics;
using Core.Systems.ObjectPools;
using UnityEngine;

namespace Gameplay.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SceneBorder : MonoBehaviour
    {
        [Header("Border Settings")]
        [SerializeField] private float _offsetFromGameFieldBorder = 4f;
        [SerializeField] private float _borderThickness = 2f;
        [SerializeField] private float _teleportOffset = 1f;

        private BorderSide _borderSide;
        private float _sceneWidth;
        private float _sceneHeight;

        public void Configure(BorderSide borderType, float sceneWidth, float sceneHeight)
        {
            _borderSide = borderType;
            _sceneWidth = sceneWidth;
            _sceneHeight = sceneHeight;

            SetupBorder();
        }
        
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position,
                new Vector2(transform.localScale.x, transform.localScale.y));
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
            else if(other.TryGetComponent(out PoolableObject poolableObject))
            {
                if(poolableObject.DespawnCondition == DespawnCondition.OutsideOfScene)
                {
                    poolableObject.Despawn();
                }
            }
        }

        private void SetupBorder()
        {
            Vector2 position = Vector2.zero;
            Vector2 size = Vector2.zero;

            float horizontalBorderLength = _sceneWidth + (_borderThickness * 2f) + (_offsetFromGameFieldBorder * 2f);
            float verticalBorderLength = _sceneHeight + (_borderThickness * 2f) + (_offsetFromGameFieldBorder * 2f);

            switch (_borderSide)
            {
                case BorderSide.Left:
                    position = new Vector2(-_sceneWidth / 2 - _offsetFromGameFieldBorder - _borderThickness / 2, 0);
                    size = new Vector2(_borderThickness, verticalBorderLength);
                    break;

                case BorderSide.Right:
                    position = new Vector2(_sceneWidth / 2 + _offsetFromGameFieldBorder + _borderThickness / 2, 0);
                    size = new Vector2(_borderThickness, verticalBorderLength);
                    break;

                case BorderSide.Top:
                    position = new Vector2(0, _sceneHeight / 2 + _offsetFromGameFieldBorder + _borderThickness / 2);
                    size = new Vector2(horizontalBorderLength, _borderThickness);
                    break;

                case BorderSide.Bottom:
                    position = new Vector2(0, -_sceneHeight / 2 - _offsetFromGameFieldBorder - _borderThickness / 2);
                    size = new Vector2(horizontalBorderLength, _borderThickness);
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
