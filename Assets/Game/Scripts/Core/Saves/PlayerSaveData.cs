using System;
using Newtonsoft.Json;

namespace Core.Saves
{
    public class PlayerSaveData
    {
        public int HighScore { get; }
        
        [JsonConstructor]
        private PlayerSaveData(int highScore, DateTime lastSaveTime)
        {
            HighScore = Math.Max(0, highScore);
        }
        
        public static PlayerSaveData CreateDefault()
        {
            return new PlayerSaveData(0, DateTime.Now);
        }
        
        public PlayerSaveData CreateWithUpdatedHighScore(int newHighScore)
        {
            if (newHighScore <= HighScore) return this;
            
            return new PlayerSaveData(newHighScore, DateTime.Now);
        }
    }
}
