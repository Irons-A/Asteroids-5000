using Newtonsoft.Json;

namespace Core.Configuration.Environment
{
    public class EnemySpawnSettings
    {
        public readonly int MaxEnemiesOnMap;
        public readonly float EnemySpawnRate;
        public readonly float EnemySpawnOffset;
        public readonly float UFOSpawnChance;
        
        [JsonConstructor]
        public EnemySpawnSettings(int maxEnemiesOnMap, float enemySpawnRate, float enemySpawnOffset, 
            float uFOSpawnChance)
        {
            MaxEnemiesOnMap = maxEnemiesOnMap;
            EnemySpawnRate = enemySpawnRate;
            EnemySpawnOffset = enemySpawnOffset;
            UFOSpawnChance = uFOSpawnChance;
        }
    }
}
