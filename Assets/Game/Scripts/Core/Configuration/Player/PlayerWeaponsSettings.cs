using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration.Player
{
    public class PlayerWeaponsSettings
    {
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
        public PlayerWeaponsSettings(float bulletFireRateInterval, float bulletSpeed, int bulletDamage, 
            float laserFireRateInterval, int maxLaserCharges, int laserAmmoPerShot, int laserAmmoPerReload, 
            float laserCooldown, float laserDuration, int laserDamage)
        {
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
