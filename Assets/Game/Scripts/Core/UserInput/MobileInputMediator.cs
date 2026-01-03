using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UserInput
{
    public class MobileInputMediator
    {
        private bool _pauseButtonPressedThisFrame;
        
        public Vector2 StickDirection { get; private set; }
        public bool IsDecelerationButtonDown { get; private set; }
        public bool IsShootBulletsButtonDown { get; private set; }
        public bool IsShootLaserButtonDown { get; private set; }
        
        public bool IsPauseButtonPressed => ConsumePauseButtonPress();
        
        public void SetStickDirection(Vector2 direction)
        {
            StickDirection = direction;
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
        
        public void SetPauseButtonPressed()
        {
            _pauseButtonPressedThisFrame = true;
        }
        
        private bool ConsumePauseButtonPress()
        {
            bool value = _pauseButtonPressedThisFrame;
            _pauseButtonPressedThisFrame = false;
            return value;
        }
        
        public void ResetAll()
        {
            StickDirection = Vector2.zero;
            IsDecelerationButtonDown = false;
            IsShootBulletsButtonDown = false;
            IsShootLaserButtonDown = false;
            _pauseButtonPressedThisFrame = false;
        }
    }
}
