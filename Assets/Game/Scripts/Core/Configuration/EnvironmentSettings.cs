using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class EnvironmentSettings
    {
        public float GameFieldWidth { get; private set; }
        public float GameFieldHeight { get; private set; }
        public float MinimalFieldWidth { get; private set; }
        public float MinimalFieldHeight { get; private set; }

        public int MaxEnemiesOnMap { get; private set; }
        public float EnemySpawnRate { get; private set; }
        public float EnemySpawnOffset { get; private set; }
        public float UFOSpawnChance { get; private set; }
        
        [JsonConstructor]
        public EnvironmentSettings(float gameFieldWidth, float gameFieldHeight, float minimalFieldWidth, 
            float minimalFieldHeight, int maxEnemiesOnMap, float enemySpawnRate, float enemySpawnOffset, 
            float uFOSpawnChance)
        {
            GameFieldWidth = gameFieldWidth;
            GameFieldHeight = gameFieldHeight;
            MinimalFieldWidth = minimalFieldWidth;
            MinimalFieldHeight = minimalFieldHeight;
            MaxEnemiesOnMap = maxEnemiesOnMap;
            EnemySpawnRate = enemySpawnRate;
            EnemySpawnOffset = enemySpawnOffset;
            UFOSpawnChance = uFOSpawnChance;
        }
    }
}
