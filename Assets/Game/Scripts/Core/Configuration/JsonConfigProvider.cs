using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Configuration
{
    public class JsonConfigProvider
    {
        public const string UserInputConfigPath = "Configs/UserInputConfig";
        public const string PlayerConfigPath = "Configs/PlayerConfig";

        public UserInputSettings LoadUserInputSettings()
        {
            return LoadConfig<UserInputSettings>(UserInputConfigPath);
        }

        public PlayerSettings LoadPlayerSettings()
        {
            return LoadConfig<PlayerSettings>(PlayerConfigPath);
        }

        private T LoadConfig<T>(string configPath) where T : class
        {
            TextAsset configFile = Resources.Load<TextAsset>(configPath);

            if (configFile == null)
            {
                Debug.LogError($"Config file not found at path: {configPath}");

                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(configFile.text);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse JSON config {configPath}: {ex.Message}");

                return null;
            }
        }
    }
}
