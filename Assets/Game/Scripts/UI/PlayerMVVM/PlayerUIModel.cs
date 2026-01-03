using System;
using System.Collections;
using System.Collections.Generic;
using Core.Configuration;
using UnityEngine;
using UniRx;

namespace UI.PlayerMVVM
{
    public class PlayerUIModel
    {
        public ReactiveProperty<int> Health { get; private set; } = new();
        public ReactiveProperty<Vector2> Coordinates { get; private set; } = new();
        public ReactiveProperty<float> PlayerAngle { get; private set; } = new();
        public ReactiveProperty<float> CurrentSpeed { get; private set; } = new();
        public ReactiveProperty<int> LaserAmmo { get; private set; } = new();
        public ReactiveProperty<float> LaserCooldown { get; private set; } = new();
        public ReactiveProperty<int> Score { get; private set; } = new();
        
        public void SetHealth(int health)
        {
            Health.Value = Math.Max(0,health);
        }
        
        public void SetCoordinates(Vector2 coordinates)
        {
            Coordinates.Value = coordinates;
        }
        
        public void SetPlayerAngle(float shipAngle)
        {
            PlayerAngle.Value = shipAngle;
        }
        
        public void SetCurrentSpeed(float immediateSpeed)
        {
            CurrentSpeed.Value = immediateSpeed;
        }
        
        public void SetLaserAmmo(int laserAmmo)
        {
            LaserAmmo.Value = laserAmmo;
        }
        
        public void SetLaserCooldown(float laserCooldown)
        {
            LaserCooldown.Value = MathF.Max(0,laserCooldown);
        }

        public void SetScore(int score)
        {
            Score.Value = score;
        }
    }
}
