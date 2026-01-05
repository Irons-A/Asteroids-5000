using UnityEngine;

namespace Gameplay.Environment.Background
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxBackgroundLayer : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private Vector2 _scrollSpeed = new Vector2(0.1f, 0.1f);
        [SerializeField] private bool _cameraRelatedParallaxEffect = true;

        public Vector2 ScrollSpeed => _scrollSpeed;
        public bool UseCameraParallax => _cameraRelatedParallaxEffect;
        public SpriteRenderer LayerRenderer { get; private set; }
        public Vector2 SavedOffset { get; private set; }

        private void Awake()
        {
            LayerRenderer = GetComponent<SpriteRenderer>();

            if (LayerRenderer != null && LayerRenderer.material != null)
            {
                SavedOffset = LayerRenderer.material.mainTextureOffset;
            }
        }

        public void UpdateTextureOffset(Vector2 newOffset)
        {
            if (LayerRenderer != null && LayerRenderer.material != null)
            {
                LayerRenderer.material.mainTextureOffset = newOffset;
            }
        }
    }
}
