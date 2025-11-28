using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Environment.Background
{
    public class BackgroundLayer : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private Vector2 _scrollSpeed = new Vector2(0.1f, 0.1f);
        [SerializeField] private bool _parallaxEffect = true;

        public Vector2 ScrollSpeed => _scrollSpeed;
        public bool UseParallax => _parallaxEffect;
        public Renderer LayerRenderer { get; private set; }
        public Vector2 SavedOffset { get; set; }

        private void Awake()
        {
            LayerRenderer = GetComponent<Renderer>();

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
