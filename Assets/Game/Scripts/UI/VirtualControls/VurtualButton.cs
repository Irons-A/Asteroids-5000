using System;
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
        [SerializeField] private VirtualButtonFunctionType buttonFunctionType = VirtualButtonFunctionType.ShootBullets;
        [SerializeField] private bool _isHoldableButton = true;

        private Button _button;
        private EventTrigger _eventTrigger;
        private bool _isPressed;
        
        public bool IsHoldableButton => _isHoldableButton;
        
        public Action<VirtualButtonFunctionType, bool> OnButtonStateChanged;
        public Action<VirtualButtonFunctionType> OnButtonPressed;
        
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
                EventTrigger.Entry pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                pointerDown.callback.AddListener((data) => OnPointerDown());
                _eventTrigger.triggers.Add(pointerDown);
                
                EventTrigger.Entry pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
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
                OnButtonStateChanged?.Invoke(buttonFunctionType, true);
            }
        }
        
        private void OnPointerUp()
        {
            if (_isPressed == false || IsHoldableButton == false) return;
            
            _isPressed = false;
            
            OnButtonStateChanged?.Invoke(buttonFunctionType, false);
        }
        
        private void OnClick()
        {
            if (IsHoldableButton == false)
            {
                OnButtonPressed?.Invoke(buttonFunctionType);
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
