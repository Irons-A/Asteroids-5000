using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class JsonConfigProvider
    {
        private const string UserInputConfigPath = "Configs/UserInputConfig";
        private const string PlayerConfigPath = "Configs/PlayerConfig";
        private const string EnvironmentConfigPath = "Configs/EnvironmentConfig";
        private const string EnemyConfigPath = "Configs/EnemyConfig";
        
        private UserInputSettings _userInputSettings;
        private PlayerSettings _playerSettings;
        private EnvironmentSettings _environmentSettings;
        private EnemySettings _enemySettings;
        
        public bool IsInitialized { get; private set; } = false;
        
        public JsonConfigProvider()
        {
            EnsureInitialized();
        }

        public UserInputSettings InputSettingsRef
        { 
            get
            {
                EnsureInitialized();
                return _userInputSettings;
            }
        }

        public PlayerSettings PlayerSettingsRef
        { 
            get
            {
                EnsureInitialized();
                return _playerSettings;
            }
        }
        
        public EnvironmentSettings EnvironmentSettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _environmentSettings;
            }
        }

        public EnemySettings EnemySettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _enemySettings;
            }
        }

        private void EnsureInitialized()
        {
            if (IsInitialized) return;
        
            LoadSettings();
            IsInitialized = true;
        }

        private void LoadSettings()
        {
            _userInputSettings = LoadConfig<UserInputSettings>(UserInputConfigPath);
            _playerSettings = LoadConfig<PlayerSettings>(PlayerConfigPath);
            _environmentSettings = LoadConfig<EnvironmentSettings>(EnvironmentConfigPath);
            _enemySettings = LoadConfig<EnemySettings>(EnemyConfigPath);
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
