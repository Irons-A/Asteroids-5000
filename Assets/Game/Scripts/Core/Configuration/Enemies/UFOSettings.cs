using Newtonsoft.Json;

namespace Core.Configuration.Enemies
{
    public class UFOSettings
    {
        public readonly float Speed;
        public readonly int Health;
        public readonly int Damage;
        public readonly int ProjectileDamage;
        public readonly float ProjectileSpeed;
        public readonly float RotationSpeed;
        public readonly float FireRateInterval;
        public readonly float Mass;
        public readonly float MinSpriteRotationSpeed;
        public readonly float MaxSpriteRotationSpeed;
        
        [JsonConstructor]
        public UFOSettings(float speed, int health, int damage, int projectileDamage, float projectileSpeed,
            float rotationSpeed, float fireRateInterval, float mass, float minSpriteRotationSpeed,
            float maxSpriteRotationSpeed)
        {
            Speed = speed;
            Health = health;
            Damage = damage;
            ProjectileDamage = projectileDamage;
            ProjectileSpeed = projectileSpeed;
            RotationSpeed = rotationSpeed;
            FireRateInterval = fireRateInterval;
            Mass = mass;
            MinSpriteRotationSpeed = minSpriteRotationSpeed;
            MaxSpriteRotationSpeed = maxSpriteRotationSpeed;
        }
    }
}
