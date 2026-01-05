using Newtonsoft.Json;

namespace Core.Configuration
{
    public class EnvironmentSettings
    {
        public readonly float GameFieldWidth;
        public readonly float GameFieldHeight;
        public readonly float MinimalFieldWidth;
        public readonly float MinimalFieldHeight;

        public readonly int MaxEnemiesOnMap;
        public readonly float EnemySpawnRate;
        public readonly float EnemySpawnOffset;
        public readonly float UFOSpawnChance;
        
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
