using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UserInput
{
    public struct JoystickData
    {
        public Vector2 Direction;
        public float Magnitude;
        public bool IsActive;
        
        public JoystickData(Vector2 direction, float magnitude, bool isActive)
        {
            Direction = direction;
            Magnitude = magnitude;
            IsActive = isActive;
        }
    }
}
