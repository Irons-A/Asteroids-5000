using System.Collections;
using System.Collections.Generic;
using Core.UserInput;
using UnityEngine;

namespace Player.UserInput.Strategies
{
    public class MobileInputStrategy //: IInputStrategy
    {
        private MobileInputMediator _mediator;

        public MobileInputStrategy(MobileInputMediator mediator)
        {
            _mediator = mediator;
        }

        // public Vector2 GetRotationInput()
        // {
        //     если есть ввод со стика, то значение. Если нет то нуль
        // }
        //
        // public PlayerMovementState GetPlayerMovementState()
        // {
        //     //если есть вращение стика - ускорение. Если нажата кнопка тормоза, тормоз
        // }
        //
        // public bool IsShootingBullets()
        // {
        //     нужно считывать пока кнопка нажата
        // }
        //
        // public bool IsShootingLaser()
        // {
        //     нужно считывать пока кнопка нажата
        // }
        //
        // public bool IsPausePressed()
        // {
        //     нужно считывать только момент нажатия
        // }
    }
}
