using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UserInput
{
    public class MobileInputMediator
    {
        public event Action<JoystickData> OnJoystickChanged;
        public event Action<VirtualButtonType> OnButtonPressed;
        public event Action<VirtualButtonType> OnButtonReleased;
        
        public void ReportJoystick(JoystickData data)
        {
            OnJoystickChanged?.Invoke(data);
        }
        
        public void ReportButtonDown(VirtualButtonType buttonId)
        {
            OnButtonPressed?.Invoke(buttonId);
        }
        
        public void ReportButtonUp(VirtualButtonType buttonId)
        {
            OnButtonReleased?.Invoke(buttonId);
        }
    }
}
