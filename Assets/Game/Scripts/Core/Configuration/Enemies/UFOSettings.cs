using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration.Enemies
{
    public class UFOSettings
    {
        public readonly float Speed;
        public readonly int Health;
        public readonly int Damage;
        public readonly int ProjectileDamage;
        public readonly float ProjectileSpeed;
        public readonly float RotationSpeed;
        public readonly float FireRateInterval;
        public readonly int Reward;
        public readonly float Mass;
        public readonly float MinSpriteRotationSpeed;
        public readonly float MaxSpriteRotationSpeed;
        
        [JsonConstructor]
        public UFOSettings(float speed, int health, int damage, int projectileDamage, float projectileSpeed,
            float rotationSpeed, float fireRateInterval, int reward, float mass, float minSpriteRotationSpeed,
            float maxSpriteRotationSpeed)
        {
            Speed = speed;
            Health = health;
            Damage = damage;
            ProjectileDamage = projectileDamage;
            ProjectileSpeed = projectileSpeed;
            RotationSpeed = rotationSpeed;
            FireRateInterval = fireRateInterval;
            Reward = reward;
            Mass = mass;
            MinSpriteRotationSpeed = minSpriteRotationSpeed;
            MaxSpriteRotationSpeed = maxSpriteRotationSpeed;
        }
    }
}
