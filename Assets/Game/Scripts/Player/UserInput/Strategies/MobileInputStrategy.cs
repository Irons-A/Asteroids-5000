using Core.UserInput;
using UnityEngine;

namespace Player.UserInput.Strategies
{
    public class MobileInputStrategy : IInputStrategy
    {
        private readonly MobileInputMediator _mediator;

        public MobileInputStrategy(MobileInputMediator mediator)
        {
            _mediator = mediator;
        }

        public Vector2 GetRotationInput()
        {
            return _mediator.StickDirection;
        }
        
        public PlayerMovementState GetPlayerMovementState()
        {
            if (_mediator.StickDirection != Vector2.zero)
            {
                return PlayerMovementState.Accelerating;
            }
            else if (_mediator.IsDecelerationButtonDown)
            {
                return PlayerMovementState.Decelerating;
            }
            else
            {
                return PlayerMovementState.Idle;
            }
        }
        
        public bool IsShootingBullets()
        {
            return _mediator.IsShootBulletsButtonDown;
        }
        
        public bool IsShootingLaser()
        {
            return _mediator.IsShootLaserButtonDown;
        }
        
        public bool IsPausePressed()
        {
            return _mediator.IsPauseButtonPressed;
        }
    }
}
