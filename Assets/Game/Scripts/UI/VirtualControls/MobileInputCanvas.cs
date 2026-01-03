using System;
using System.Collections;
using System.Collections.Generic;
using Core.UserInput;
using UnityEngine;
using Zenject;

namespace UI.VirtualControls
{
    public class MobileInputCanvas : MonoBehaviour
    {
        [SerializeField] private VirtualJoystick _joystick;
        [SerializeField] private VirtualButton _shootBulletsButton;
        [SerializeField] private VirtualButton _shootLaserButton;
        [SerializeField] private VirtualButton _decelerationButton;
        [SerializeField] private VirtualButton _pauseButton;
        
        private MobileInputMediator _mediator;
        
        [Inject]
        private void Construct(MobileInputMediator mediator)
        {
            _mediator = mediator;
            SetupControls();
        }

        private void OnEnable()
        {
            if (_joystick != null)
            {
                _joystick.OnValueChanged += SetStickDirection;
            }
        }
        
        private void OnDisable()
        {
            if (_joystick != null)
            {
                _joystick.OnValueChanged -= SetStickDirection;
            }
            
            _mediator?.ResetAll();
        }

        private void SetupControls()
        {
            SetupVirtualButton(_shootBulletsButton, VirtualButtonFunctionType.ShootBullets);
            SetupVirtualButton(_shootLaserButton, VirtualButtonFunctionType.ShootLaser);
            SetupVirtualButton(_decelerationButton, VirtualButtonFunctionType.Decelerate);
            SetupVirtualButton(_pauseButton, VirtualButtonFunctionType.Pause);
        }
        
        private void SetupVirtualButton(VirtualButton virtualButton, VirtualButtonFunctionType expectedFunctionType)
        {
            if (virtualButton == null) return;
            
            if (virtualButton.IsHoldableButton)
            {
                virtualButton.OnButtonStateChanged += (buttonType, isPressed) =>
                {
                    HandleHoldButton(buttonType, isPressed);
                };
            }
            else
            {
                virtualButton.OnButtonPressed += (buttonType) =>
                {
                    HandlePressButton(buttonType);
                };
            }
        }

        private void SetStickDirection(Vector2 direction)
        {
            _mediator.SetStickDirection(direction);
        }
        
        private void HandleHoldButton(VirtualButtonFunctionType buttonFunctionType, bool isPressed)
        {
            switch (buttonFunctionType)
            {
                case VirtualButtonFunctionType.ShootBullets:
                    _mediator.SetIsShootBulletsButtonDown(isPressed);
                    break;
                    
                case VirtualButtonFunctionType.ShootLaser:
                    _mediator.SetIsShootLaserButtonDown(isPressed);
                    break;
                    
                case VirtualButtonFunctionType.Decelerate:
                    _mediator.SetIsDecelerationButtonDown(isPressed);
                    break;
                    
                default:
                    Debug.LogWarning($"Unhandled hold button type: {buttonFunctionType}");
                    break;
            }
        }
        
        private void HandlePressButton(VirtualButtonFunctionType buttonFunctionType)
        {
            switch (buttonFunctionType)
            {
                case VirtualButtonFunctionType.Pause:
                    _mediator.SetPauseButtonPressed();
                    break;
                    
                default:
                    Debug.LogWarning($"Unhandled press button type: {buttonFunctionType}");
                    break;
            }
        }
    }
}
