using Newtonsoft.Json;

namespace Core.Configuration.Enemies
{
    public class EnemyRewardsSettings
    {
        public readonly int BigAsteroidReward;
        public readonly int SmallAsteroidReward;
        public readonly int UFOReward;

        [JsonConstructor]
        public EnemyRewardsSettings(int bigAsteroidReward, int smallAsteroidReward, int uFOReward)
        {
            BigAsteroidReward = bigAsteroidReward;
            SmallAsteroidReward = smallAsteroidReward;
            UFOReward = uFOReward;
        }
    }
}
