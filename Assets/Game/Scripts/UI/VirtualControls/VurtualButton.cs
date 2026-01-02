using System;
using System.Collections;
using System.Collections.Generic;
using Core.UserInput;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.VirtualControls
{
    [RequireComponent(typeof(Button))]
    public class VirtualButton : MonoBehaviour
    {
        [SerializeField] private VirtualButtonType _buttonType = VirtualButtonType.ShootBullets;

        private Button _button;
        private bool _isPressed;
        
        [field: SerializeField] public bool IsHoldableButton { get; private set; } = true;
        
        // События
        public System.Action<VirtualButtonType, bool> OnButtonStateChanged; // Для Hold кнопок
        public System.Action<VirtualButtonType> OnButtonPressed; // Для Press кнопок
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            SetupButtonListeners();
        }
        
        private void SetupButtonListeners()
        {
            if (_button == null) return;
            
            // Используем EventTrigger для отслеживания нажатия и отпускания
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            
            // Нажатие
            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((data) => OnPointerDown());
            eventTrigger.triggers.Add(pointerDown);
            
            // Отпускание (только для удерживаемых кнопок)
            if (IsHoldableButton)
            {
                var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                pointerUp.callback.AddListener((data) => OnPointerUp());
                eventTrigger.triggers.Add(pointerUp);
                
                var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                pointerExit.callback.AddListener((data) => OnPointerUp());
                eventTrigger.triggers.Add(pointerExit);
            }
            
            // Для однократных кнопок используем стандартный onClick
            if (!IsHoldableButton)
            {
                _button.onClick.AddListener(OnClick);
            }
        }
        
        private void OnPointerDown()
        {
            if (_isPressed) return;
            
            _isPressed = true;
            
            if (IsHoldableButton)
            {
                OnButtonStateChanged?.Invoke(_buttonType, true);
            }
            else
            {
                OnButtonPressed?.Invoke(_buttonType);
            }
        }
        
        private void OnPointerUp()
        {
            if (!_isPressed || !IsHoldableButton) return;
            
            _isPressed = false;
            OnButtonStateChanged?.Invoke(_buttonType, false);
        }
        
        private void OnClick()
        {
            // Для однократных кнопок, обрабатываемых через стандартный onClick
            if (!IsHoldableButton)
            {
                OnButtonPressed?.Invoke(_buttonType);
            }
        }
        
        // Публичный метод для программного нажатия
        public void SimulatePress()
        {
            if (IsHoldableButton)
            {
                OnPointerDown();
            }
            else
            {
                OnClick();
            }
        }
        
        // Публичный метод для программного отпускания
        public void SimulateRelease()
        {
            if (IsHoldableButton)
            {
                OnPointerUp();
            }
        }
        
        private void OnDestroy()
        {
            if (_button != null && !IsHoldableButton)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }
    }
}
