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
        
        // Ссылки на кнопки через VirtualButton компонент
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
        
        private void SetupControls()
        {
            // Настройка джойстика
            if (_joystick != null)
            {
                _joystick.OnValueChanged += (direction, magnitude) =>
                {
                    _mediator.SetStickDirection(direction, magnitude);
                };
            }
            
            // Настройка виртуальных кнопок
            SetupVirtualButton(_shootBulletsButton, VirtualButtonType.ShootBullets);
            SetupVirtualButton(_shootLaserButton, VirtualButtonType.ShootLaser);
            SetupVirtualButton(_decelerationButton, VirtualButtonType.Decelerate);
            SetupVirtualButton(_pauseButton, VirtualButtonType.Pause);
        }
        
        private void SetupVirtualButton(VirtualButton virtualButton, VirtualButtonType expectedType)
        {
            if (virtualButton == null) return;
            
            // Подписываемся на события кнопки
            if (virtualButton.IsHoldableButton)
            {
                // Для удерживаемых кнопок
                virtualButton.OnButtonStateChanged += (buttonType, isPressed) =>
                {
                    HandleHoldButton(buttonType, isPressed);
                };
            }
            else
            {
                // Для однократных кнопок
                virtualButton.OnButtonPressed += (buttonType) =>
                {
                    HandlePressButton(buttonType);
                };
            }
        }
        
        private void HandleHoldButton(VirtualButtonType buttonType, bool isPressed)
        {
            switch (buttonType)
            {
                case VirtualButtonType.ShootBullets:
                    _mediator.SetIsShootBulletsButtonDown(isPressed);
                    break;
                    
                case VirtualButtonType.ShootLaser:
                    _mediator.SetIsShootLaserButtonDown(isPressed);
                    break;
                    
                case VirtualButtonType.Decelerate:
                    _mediator.SetIsDecelerationButtonDown(isPressed);
                    break;
                    
                default:
                    Debug.LogWarning($"Unhandled hold button type: {buttonType}");
                    break;
            }
        }
        
        private void HandlePressButton(VirtualButtonType buttonType)
        {
            switch (buttonType)
            {
                case VirtualButtonType.Pause:
                    _mediator.SetPauseButtonPressed();
                    break;
                    
                default:
                    Debug.LogWarning($"Unhandled press button type: {buttonType}");
                    break;
            }
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_joystick != null)
            {
                //_joystick.OnValueChanged = null;
            }
            
            // Можно отписаться от кнопок, но при уничтожении объекта это не обязательно
        }
        
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            if (!active)
            {
                _mediator?.ResetAll();
            }
        }
    }
}
