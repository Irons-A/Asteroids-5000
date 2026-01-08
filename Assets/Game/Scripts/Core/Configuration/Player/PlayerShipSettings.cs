using Newtonsoft.Json;

namespace Core.Configuration.Player
{
    public class PlayerShipSettings
    {
        public readonly float AccelerationSpeed;
        public readonly float DecelerationSpeed;
        public readonly float MaxSpeed;
        public readonly float RotationSpeed;
        public readonly int MaxHealth;
        public readonly float UncontrollabilityDuration;
        public readonly float InvulnerabilityDuration;
        public readonly float PlayerMass;

        [JsonConstructor]
        public PlayerShipSettings(float accelerationSpeed, float decelerationSpeed, float maxSpeed,
            float rotationSpeed, int maxHealth, float uncontrollabilityDuration, float invulnerabilityDuration,
            float playerMass)
        {
            AccelerationSpeed = accelerationSpeed;
            DecelerationSpeed = decelerationSpeed;
            MaxSpeed = maxSpeed;
            RotationSpeed = rotationSpeed;
            MaxHealth = maxHealth;
            UncontrollabilityDuration = uncontrollabilityDuration;
            InvulnerabilityDuration = invulnerabilityDuration;
            PlayerMass = playerMass;
        }
    }
}
