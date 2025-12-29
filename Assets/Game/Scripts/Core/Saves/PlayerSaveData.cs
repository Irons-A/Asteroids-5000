using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

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
        
        public PlayerSaveData WithUpdatedHighScore(int newHighScore)
        {
            if (newHighScore <= HighScore) return this;
            
            return new PlayerSaveData(newHighScore, DateTime.Now);
        }
    }
}
