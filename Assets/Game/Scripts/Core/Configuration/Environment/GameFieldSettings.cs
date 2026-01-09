using Newtonsoft.Json;

namespace Core.Configuration.Environment
{
    public class GameFieldSettings
    {
        public readonly float GameFieldWidth;
        public readonly float GameFieldHeight;
        public readonly float MinimalFieldWidth;
        public readonly float MinimalFieldHeight;
        
        [JsonConstructor]
        public GameFieldSettings(float gameFieldWidth, float gameFieldHeight, float minimalFieldWidth, 
            float minimalFieldHeight)
        {
            GameFieldWidth = gameFieldWidth;
            GameFieldHeight = gameFieldHeight;
            MinimalFieldWidth = minimalFieldWidth;
            MinimalFieldHeight = minimalFieldHeight;
        }
    }
}
