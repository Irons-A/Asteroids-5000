using Newtonsoft.Json;

namespace Core.Configuration.Enemies
{
    public class BigAsteroidSettings
    {
        public readonly float Speed;
        public readonly int Health;
        public readonly int Damage;
        public readonly int MinSmallAsteroidSpawnAmount;
        public readonly int MaxSmallAsteroidSpawnAmount;
        public readonly float Mass;
        public readonly float MinSpriteRotationSpeed;
        public readonly float MaxSpriteRotationSpeed;
        
        [JsonConstructor]
        public BigAsteroidSettings(float speed, int health, int damage, int minSmallAsteroidSpawnAmount,
            int maxSmallAsteroidSpawnAmount,float mass, float minSpriteRotationSpeed, float maxSpriteRotationSpeed)
        {
            Speed = speed;
            Health = health;
            Damage = damage;
            MinSmallAsteroidSpawnAmount = minSmallAsteroidSpawnAmount;
            MaxSmallAsteroidSpawnAmount = maxSmallAsteroidSpawnAmount;
            Mass = mass;
            MinSpriteRotationSpeed = minSpriteRotationSpeed;
            MaxSpriteRotationSpeed = maxSpriteRotationSpeed;
        }
    }
}
