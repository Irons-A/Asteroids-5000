using System;
using System.Collections;
using System.Collections.Generic;
using Core.UserInput;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.VirtualControls
{
    public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private VirtualButtonType _buttonId;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Color _pressedColor = new Color(0.8f, 0.8f, 0.8f);
        
        private Color _originalColor;
        private bool _isPressed;
        
        public event Action<VirtualButtonType> OnButtonDown;
        public event Action<VirtualButtonType> OnButtonUp;
        
        private void Awake()
        {
            _originalColor = _buttonImage.color;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPressed) return;
            
            _isPressed = true;
            _buttonImage.color = _pressedColor;
            OnButtonDown?.Invoke(_buttonId);
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            
            _isPressed = false;
            _buttonImage.color = _originalColor;
            OnButtonUp?.Invoke(_buttonId);
        }
    }
}
