using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class PlayerSettings
    {
        public float AccelerationSpeed { get; private set; }
        public float DecelerationSpeed { get; private set; }
        public float MaxSpeed { get; private set; }
        public float RotationSpeed { get; private set; }
        public int MaxHealth { get; private set; }
        public float UncontrollabilityDuration { get; private set; }
        public float InvulnerabilityDuration { get; private set; }

        public float BulletFireRateInterval { get; private set; }
        public float BulletSpeed { get; private set; }
        public int BulletDamage { get; private set; }
        public float LaserFireRateInterval { get; private set; }
        public int MaxLaserCharges { get; private set; }
        public int LaserAmmoPerShot { get; private set; }
        public int LaserAmmoPerReload { get; private set; }
        public float LaserCooldown { get; private set; }
        public float LaserDuration { get; private set; }
        public int LaserDamage { get; private set; }

        [JsonConstructor]
        public PlayerSettings(float accelerationSpeed, float decelerationSpeed, float maxSpeed, float rotationSpeed,
            int maxHealth, float uncontrollabilityDuration, float invulnerabilityDuration, float bulletFireRateInterval,
            float bulletSpeed, int bulletDamage, float laserFireRateInterval, int maxLaserCharges, int laserAmmoPerShot,
            int laserAmmoPerReload, float laserCooldown, float laserDuration, int laserDamage)
        {
            AccelerationSpeed = accelerationSpeed;
            DecelerationSpeed = decelerationSpeed;
            MaxSpeed = maxSpeed;
            RotationSpeed = rotationSpeed;
            MaxHealth = maxHealth;
            UncontrollabilityDuration = uncontrollabilityDuration;
            InvulnerabilityDuration = invulnerabilityDuration;
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
