using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Configuration
{
    public class JsonConfigProvider
    {
        public const string PlayerInputConfigPath = "Configs/PlayerInputConfig";

        public PlayerInputSettings LoadPlayerInputSettings()
        {
            return LoadConfig<PlayerInputSettings>(PlayerInputConfigPath);
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
