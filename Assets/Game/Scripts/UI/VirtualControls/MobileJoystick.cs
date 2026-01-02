using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.VirtualControls
{
    public class MobileJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _maxRadius = 100f;
        
        private Vector2 _originalPosition;
        
        public event System.Action<Vector2, float> OnValueChanged;
        
        private void Awake()
        {
            _originalPosition = _background.anchoredPosition;
            ResetJoystick();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            UpdateJoystick(eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            UpdateJoystick(eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            ResetJoystick();
        }
        
        private void UpdateJoystick(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background.parent as RectTransform,
                screenPosition,
                null,
                out Vector2 localPoint
            );
            
            Vector2 direction = localPoint - _originalPosition;
            float distance = Mathf.Min(direction.magnitude, _maxRadius);
            
            // Позиция ручки
            _handle.anchoredPosition = direction.normalized * distance;
            
            // Нормализованные значения
            float magnitude = distance / _maxRadius;
            Vector2 normalizedDirection = direction.normalized;
            
            OnValueChanged?.Invoke(normalizedDirection, magnitude);
        }
        
        private void ResetJoystick()
        {
            _handle.anchoredPosition = Vector2.zero;
            OnValueChanged?.Invoke(Vector2.zero, 0);
        }
    }
}
