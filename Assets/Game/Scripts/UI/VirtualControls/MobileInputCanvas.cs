using System.Collections;
using System.Collections.Generic;
using Core.UserInput;
using UnityEngine;
using Zenject;

namespace UI.VirtualControls
{
    public class MobileInputCanvas : MonoBehaviour
    {
        [SerializeField] private MobileJoystick _joystick;
        [SerializeField] private MobileButton _decelerationButton;
        [SerializeField] private MobileButton _shootBulletsButton;
        [SerializeField] private MobileButton _shootLaserButton;
        [SerializeField] private MobileButton _pauseButton;
        
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
            
            // Настройка кнопок с удержанием
            if (_decelerationButton != null)
            {
                _decelerationButton.OnButtonStateChanged += (buttonId) =>
                {
                    bool isPressed = !buttonId.EndsWith("_up");
                    _mediator.SetIsDecelerationButtonDown(isPressed);
                };
            }
            
            if (_shootBulletsButton != null)
            {
                _shootBulletsButton.OnButtonStateChanged += (buttonId) =>
                {
                    bool isPressed = !buttonId.EndsWith("_up");
                    _mediator.SetIsShootBulletsButtonDown(isPressed);
                };
            }
            
            if (_shootLaserButton != null)
            {
                _shootLaserButton.OnButtonStateChanged += (buttonId) =>
                {
                    bool isPressed = !buttonId.EndsWith("_up");
                    _mediator.SetIsShootLaserButtonDown(isPressed);
                };
            }
            
            // Настройка кнопки с моментальным нажатием
            if (_pauseButton != null)
            {
                _pauseButton.OnButtonPressed += (buttonId) =>
                {
                    //_mediator.SetIsPauseButtonPressed();
                };
            }
        }
        
        private void OnDestroy()
        {
            // Отписываемся от событий
            if (_joystick != null)
            {
                //_joystick.OnValueChanged = null;
            }
            
            // Можно также отписаться от кнопок, но при уничтожении канваса это не обязательно
        }
        
        // Метод для включения/отключения управления
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            if (!active)
            {
                // При отключении сбрасываем все состояния
                _mediator?.ResetAll();
            }
        }
    }
}
