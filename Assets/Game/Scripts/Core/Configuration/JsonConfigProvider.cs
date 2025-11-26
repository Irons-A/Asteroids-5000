using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Configuration
{
    public class JsonConfigProvider : IInitializable
    {
        public const string UserInputConfigPath = "Configs/UserInputConfig";
        public const string PlayerConfigPath = "Configs/PlayerConfig";

        public UserInputSettings InputSettingsRef { get; private set; }
        public PlayerSettings PlayerSettingsRef { get; private set; }

        public void Initialize()
        {
            LoadUserInputSettings();
            LoadPlayerSettings();
        }

        public void LoadUserInputSettings()
        {
            InputSettingsRef = LoadConfig<UserInputSettings>(UserInputConfigPath);
        }

        public void LoadPlayerSettings()
        {
            PlayerSettingsRef = LoadConfig<PlayerSettings>(PlayerConfigPath);
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
