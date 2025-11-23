using Core.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.UserInput
{
    public interface IInputStrategy
    {
        public Vector2 GetRotationInput();
        public PlayerMovementState GetPlayerMovementState();
        public bool IsShootingBullets();
        public bool IsShootingLaser();
        public bool IsPausePressed();
    }
}
