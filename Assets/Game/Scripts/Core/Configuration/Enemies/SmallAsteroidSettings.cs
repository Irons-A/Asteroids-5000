using Newtonsoft.Json;

namespace Core.Configuration.Enemies
{
    public class SmallAsteroidSettings
    {
        public readonly float Speed;
        public readonly int Health;
        public readonly int Damage;
        public readonly float Mass;
        public readonly float MinSpriteRotationSpeed;
        public readonly float MaxSpriteRotationSpeed;

        [JsonConstructor]
        public SmallAsteroidSettings(float speed, int health, int damage, float mass, float minSpriteRotationSpeed,
            float maxSpriteRotationSpeed)
        {
            Speed = speed;
            Health = health;
            Damage = damage;
            Mass = mass;
            MinSpriteRotationSpeed = minSpriteRotationSpeed;
            MaxSpriteRotationSpeed = maxSpriteRotationSpeed;
        }
    }
}
