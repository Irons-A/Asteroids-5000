using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Configuration
{
    public class PlayerSettings
    {
        public float AccelerationSpeed;
        public float DecelerationSpeed;
        public float MaxSpeed;
        public float RotationSpeed;
        public int MaxHealth;
        public float UncontrollabilityDuration;
        public float InvulnerabilityDuration;

        public float BulletFireRateInterval;
        public float BulletSpeed;
        public int BulletDamage;
        public float LaserFireRateInterval;
        public int MaxLaserCharges;
        public int LaserAmmoPerShot;
        public int LaserAmmoPerReload;
        public float LaserCooldown;
        public float LaserDuration;
        public int LaserDamage;
    }
}
