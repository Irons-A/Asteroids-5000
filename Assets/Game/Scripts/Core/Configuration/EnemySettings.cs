using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Configuration
{
    public class EnemySettings
    {
        public float BigAsteroidSpeed;
        public int BigAsteroidHealth;
        public int BigAsteroidDamage;
        public int MinSmallAsteroidSpawnAmount;
        public int MaxSmallAsteroidSpawnAmount;
        
        public float SmallAsteroidSpeed;
        public int SmallAsteroidHealth;
        public int SmallAsteroidDamage;

        public float UFOSpeed;
        public int UFOHealth;
        public int UFODamage;
        public int UFOProjectileDamage;
        public float UFORotationSpeed;
        public float UFOFireRateInterval;
    }
}
