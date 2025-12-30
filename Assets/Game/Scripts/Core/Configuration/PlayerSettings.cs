using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class PlayerSettings
    {
        public readonly float AccelerationSpeed;
        public readonly float DecelerationSpeed;
        public readonly float MaxSpeed;
        public readonly float RotationSpeed;
        public readonly int MaxHealth;
        public readonly float UncontrollabilityDuration;
        public readonly float InvulnerabilityDuration;
        public readonly float PlayerMass;

        public readonly float BulletFireRateInterval;
        public readonly float BulletSpeed;
        public readonly int BulletDamage;
        public readonly float LaserFireRateInterval;
        public readonly int MaxLaserCharges;
        public readonly int LaserAmmoPerShot;
        public readonly int LaserAmmoPerReload;
        public readonly float LaserCooldown;
        public readonly float LaserDuration;
        public readonly int LaserDamage;

        [JsonConstructor]
        public PlayerSettings(float accelerationSpeed, float decelerationSpeed, float maxSpeed, float rotationSpeed,
            int maxHealth, float uncontrollabilityDuration, float invulnerabilityDuration, float playerMass,
            float bulletFireRateInterval, float bulletSpeed, int bulletDamage, float laserFireRateInterval, 
            int maxLaserCharges, int laserAmmoPerShot, int laserAmmoPerReload, float laserCooldown, 
            float laserDuration, int laserDamage)
        {
            AccelerationSpeed = accelerationSpeed;
            DecelerationSpeed = decelerationSpeed;
            MaxSpeed = maxSpeed;
            RotationSpeed = rotationSpeed;
            MaxHealth = maxHealth;
            UncontrollabilityDuration = uncontrollabilityDuration;
            InvulnerabilityDuration = invulnerabilityDuration;
            PlayerMass = playerMass;
            BulletFireRateInterval = bulletFireRateInterval;
            BulletSpeed = bulletSpeed;
            BulletDamage = bulletDamage;
            LaserFireRateInterval = laserFireRateInterval;
            MaxLaserCharges = maxLaserCharges;
            LaserAmmoPerShot = laserAmmoPerShot;
            LaserAmmoPerReload = laserAmmoPerReload;
            LaserCooldown = laserCooldown;
            LaserDuration = laserDuration;
            LaserDamage = laserDamage;
        }
    }
}
