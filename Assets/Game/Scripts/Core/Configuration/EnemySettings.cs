using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class EnemySettings
    {
        public float BigAsteroidSpeed { get; private set; }
        public int BigAsteroidHealth { get; private set; }
        public int BigAsteroidDamage { get; private set; }
        public int MinSmallAsteroidSpawnAmount { get; private set; }
        public int MaxSmallAsteroidSpawnAmount { get; private set; }
        public int BigAsteroidReward { get; private set; }
        
        public float SmallAsteroidSpeed { get; private set; }
        public int SmallAsteroidHealth { get; private set; }
        public int SmallAsteroidDamage { get; private set; }
        public int SmallAsteroidReward { get; private set; }

        public float UFOSpeed { get; private set; }
        public int UFOHealth { get; private set; }
        public int UFODamage { get; private set; }
        public int UFOProjectileDamage { get; private set; }
        public float UFOProjectileSpeed { get; private set; }
        public float UFORotationSpeed { get; private set; }
        public float UFOFireRateInterval { get; private set; }
        public int UFOReward { get; private set; }

        [JsonConstructor]
        public EnemySettings(float bigAsteroidSpeed, int bigAsteroidHealth, int bigAsteroidDamage,
            int minSmallAsteroidSpawnAmount, int maxSmallAsteroidSpawnAmount, int bigAsteroidReward,
            float smallAsteroidSpeed, int smallAsteroidHealth, int smallAsteroidDamage, int smallAsteroidReward,
            float uFOSpeed, int uFOHealth, int uFODamage, int uFOProjectileDamage, float uFOProjectileSpeed,
            float uFORotationSpeed, float uFOFireRateInterval, int uFOReward)
        {
            BigAsteroidSpeed = bigAsteroidSpeed;
            BigAsteroidHealth = bigAsteroidHealth;
            BigAsteroidDamage = bigAsteroidDamage;
            MinSmallAsteroidSpawnAmount = minSmallAsteroidSpawnAmount;
            MaxSmallAsteroidSpawnAmount = maxSmallAsteroidSpawnAmount;
            BigAsteroidReward = bigAsteroidReward;
            
            SmallAsteroidSpeed = smallAsteroidSpeed;
            SmallAsteroidHealth = smallAsteroidHealth;
            SmallAsteroidDamage = smallAsteroidDamage;
            SmallAsteroidReward = smallAsteroidReward;
            
            UFOSpeed = uFOSpeed;
            UFOHealth = uFOHealth;
            UFODamage = uFODamage;
            UFOProjectileDamage = uFOProjectileDamage;
            UFOProjectileSpeed = uFOProjectileSpeed;
            UFORotationSpeed = uFORotationSpeed;
            UFOFireRateInterval = uFOFireRateInterval;
            UFOReward = uFOReward;
        }
    }
}
