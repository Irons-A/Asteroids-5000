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
        public enum ButtonType
        {
            Hold,   // Кнопка с удержанием
            Press   // Кнопка с моментальным нажатием
        }
        
        [SerializeField] private string _buttonId = "Fire";
        [SerializeField] private ButtonType _buttonType = ButtonType.Hold;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Color _pressedColor = new Color(0.8f, 0.8f, 0.8f);
        
        private Color _originalColor;
        private bool _isPressed;
        
        // События для разных типов кнопок
        public event System.Action<string> OnButtonStateChanged; // Для Hold кнопок (состояние)
        public event System.Action<string> OnButtonPressed;      // Для Press кнопок (момент нажатия)
        
        private void Awake()
        {
            _originalColor = _buttonImage.color;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isPressed) return;
            
            _isPressed = true;
            _buttonImage.color = _pressedColor;
            
            if (_buttonType == ButtonType.Hold)
            {
                OnButtonStateChanged?.Invoke(_buttonId);
            }
            else if (_buttonType == ButtonType.Press)
            {
                OnButtonPressed?.Invoke(_buttonId);
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            
            _isPressed = false;
            _buttonImage.color = _originalColor;
            
            // Для Hold кнопок отправляем отжатое состояние
            if (_buttonType == ButtonType.Hold)
            {
                OnButtonStateChanged?.Invoke(_buttonId + "_up");
            }
            // Для Press кнопок не нужно отправлять ничего при отпускании
        }
    }
}
