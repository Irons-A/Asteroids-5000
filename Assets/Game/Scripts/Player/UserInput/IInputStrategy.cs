using UnityEngine;

namespace Player.UserInput
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
