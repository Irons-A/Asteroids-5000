using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRotator : MonoBehaviour
    {
        [SerializeField] private float _minSpeed = 60f;
        [SerializeField] private float _maxSpeed = 150f;
        
        private Transform _transform;
        private SpriteRenderer _spriteRenderer;
        
        private float _speed;

        private void Awake()
        {
            _transform = transform;
            _spriteRenderer  = GetComponentInChildren<SpriteRenderer>();
            
            _speed = Random.Range(_minSpeed, _maxSpeed);
        }

        private void Update()
        {
            if (_spriteRenderer.isVisible)
            {
                _transform.Rotate(0,0, _speed * Time.deltaTime);
            }
        }

        public void SetParameters(float minSpeed, float maxSpeed)
        {
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;
        }
    }
}
