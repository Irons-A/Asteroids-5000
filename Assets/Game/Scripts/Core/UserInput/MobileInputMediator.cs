using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UserInput
{
    public class MobileInputMediator
    {
        public Vector2 StickDirection { get; private set; }
        public bool IsDecelerationButtonDown { get; private set; }
        public bool IsShootBulletsButtonDown { get; private set; }
        public bool IsShootLaserButtonDown { get; private set; }
        public bool IsPauseButtonPressed { get; private set; }
        
        private const float STICK_DEADZONE = 0.2f;
        
        public void SetStickDirection(Vector2 direction, float magnitude)
        {
            if (magnitude < STICK_DEADZONE)
            {
                StickDirection = Vector2.zero;
            }
            else
            {
                StickDirection = direction;
            }
        }

        public void SetIsDecelerationButtonDown(bool value)
        {
            IsDecelerationButtonDown = value;
        }

        public void SetIsShootBulletsButtonDown(bool value)
        {
            IsShootBulletsButtonDown = value;
        }
        
        public void SetIsShootLaserButtonDown(bool value)
        {
            IsShootLaserButtonDown = value;
        }
        
        public void SetIsPauseButtonPressed(bool value)
        {
            IsPauseButtonPressed = value;
        }
        
        public void ResetAll()
        {
            StickDirection = Vector2.zero;
            IsDecelerationButtonDown = false;
            IsShootBulletsButtonDown = false;
            IsShootLaserButtonDown = false;
            //_pauseButtonPressedThisFrame = false;
        }
    }
}
