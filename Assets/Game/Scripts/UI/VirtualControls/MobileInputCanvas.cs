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
        [SerializeField] private MobileButton[] _buttons;
        
        private MobileInputMediator _mediator;
        
        [Inject]
        private void Construct(MobileInputMediator mediator)
        {
            _mediator = mediator;
            SetupEvents();
        }
        
        private void SetupEvents()
        {
            if (_joystick != null)
            {
                _joystick.OnValueChanged += (direction, magnitude) =>
                {
                    var data = new JoystickData(direction, magnitude, magnitude > 0);
                    _mediator.ReportJoystick(data);
                };
            }
            
            foreach (var button in _buttons)
            {
                button.OnButtonDown += _mediator.ReportButtonDown;
                button.OnButtonUp += _mediator.ReportButtonUp;
            }
        }
        
        private void OnDestroy()
        {
            if (_joystick != null)
            {
                //_joystick.OnValueChanged = null;
            }
            
            foreach (var button in _buttons)
            {
                button.OnButtonDown -= _mediator.ReportButtonDown;
                button.OnButtonUp -= _mediator.ReportButtonUp;
            }
        }
    }
}
