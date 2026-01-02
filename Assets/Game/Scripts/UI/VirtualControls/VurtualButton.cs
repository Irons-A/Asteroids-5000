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
    [RequireComponent(typeof(EventTrigger))]
    public class VirtualButton : MonoBehaviour
    {
        [SerializeField] private VirtualButtonType _buttonType = VirtualButtonType.ShootBullets;

        private Button _button;
        private EventTrigger _eventTrigger;
        private bool _isPressed;
        
        [field: SerializeField] public bool IsHoldableButton { get; private set; } = true;
        
        public Action<VirtualButtonType, bool> OnButtonStateChanged;
        public Action<VirtualButtonType> OnButtonPressed;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _eventTrigger = GetComponent<EventTrigger>();
            
            SetupButtonListeners();
        }
        
        private void SetupButtonListeners()
        {
            if (_button == null) return;
            
            if (IsHoldableButton)
            {
                var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                pointerDown.callback.AddListener((data) => OnPointerDown());
                _eventTrigger.triggers.Add(pointerDown);
                
                var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                pointerUp.callback.AddListener((data) => OnPointerUp());
                _eventTrigger.triggers.Add(pointerUp);
            }
            else
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
        }
        
        private void OnPointerUp()
        {
            if (_isPressed == false || IsHoldableButton == false) return;
            
            _isPressed = false;
            OnButtonStateChanged?.Invoke(_buttonType, false);
        }
        
        private void OnClick()
        {
            if (IsHoldableButton == false)
            {
                OnButtonPressed?.Invoke(_buttonType);
            }
        }
        
        private void OnDestroy()
        {
            if (_button != null && IsHoldableButton == false)
            {
                _button.onClick.RemoveListener(OnClick);
            }
        }
    }
}
