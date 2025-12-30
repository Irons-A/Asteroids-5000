using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class EnemySettings
    {
        public readonly float BigAsteroidSpeed;
        public readonly int BigAsteroidHealth;
        public readonly int BigAsteroidDamage;
        public readonly int MinSmallAsteroidSpawnAmount;
        public readonly int MaxSmallAsteroidSpawnAmount;
        public readonly int BigAsteroidReward;
        public readonly float BigAsteroidMass;

        public readonly float SmallAsteroidSpeed;
        public readonly int SmallAsteroidHealth;
        public readonly int SmallAsteroidDamage;
        public readonly int SmallAsteroidReward;
        public readonly float SmallAsteroidMass;

        public readonly float UFOSpeed;
        public readonly int UFOHealth;
        public readonly int UFODamage;
        public readonly int UFOProjectileDamage;
        public readonly float UFOProjectileSpeed;
        public readonly float UFORotationSpeed;
        public readonly float UFOFireRateInterval;
        public readonly int UFOReward;
        public readonly float UFOMass;

        [JsonConstructor]
        public EnemySettings(float bigAsteroidSpeed, int bigAsteroidHealth, int bigAsteroidDamage,
            int minSmallAsteroidSpawnAmount, int maxSmallAsteroidSpawnAmount, int bigAsteroidReward,
            float bigAsteroidMass, float smallAsteroidSpeed, int smallAsteroidHealth, int smallAsteroidDamage, 
            int smallAsteroidReward, float smallAsteroidMass, float uFOSpeed, int uFOHealth, int uFODamage, 
            int uFOProjectileDamage, float uFOProjectileSpeed, float uFORotationSpeed, float uFOFireRateInterval,
            int uFOReward, float uFOMass)
        {
            BigAsteroidSpeed = bigAsteroidSpeed;
            BigAsteroidHealth = bigAsteroidHealth;
            BigAsteroidDamage = bigAsteroidDamage;
            MinSmallAsteroidSpawnAmount = minSmallAsteroidSpawnAmount;
            MaxSmallAsteroidSpawnAmount = maxSmallAsteroidSpawnAmount;
            BigAsteroidReward = bigAsteroidReward;
            BigAsteroidMass = bigAsteroidMass;
            
            SmallAsteroidSpeed = smallAsteroidSpeed;
            SmallAsteroidHealth = smallAsteroidHealth;
            SmallAsteroidDamage = smallAsteroidDamage;
            SmallAsteroidReward = smallAsteroidReward;
            SmallAsteroidMass = smallAsteroidMass;
            
            UFOSpeed = uFOSpeed;
            UFOHealth = uFOHealth;
            UFODamage = uFODamage;
            UFOProjectileDamage = uFOProjectileDamage;
            UFOProjectileSpeed = uFOProjectileSpeed;
            UFORotationSpeed = uFORotationSpeed;
            UFOFireRateInterval = uFOFireRateInterval;
            UFOReward = uFOReward;
            UFOMass = uFOMass;
        }
    }
}
