using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core.Configuration
{
    public class JsonConfigProvider
    {
        public const string UserInputConfigPath = "Configs/UserInputConfig";
        public const string PlayerConfigPath = "Configs/PlayerConfig";
        public const string EnvironmentConfigPath = "Configs/EnvironmentConfig";
        public const string EnemyConfigPath = "Configs/EnemyConfig";
        
        private UserInputSettings _userInputSettings;
        private PlayerSettings _playerSettings;
        private EnvironmentSettings _environmentSettings;
        private EnemySettings _enemySettings;
        private bool _isInitialized = false;
        
        [Inject]
        private void Construct()
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
            if (_isInitialized) return;
        
            LoadSettings();
            _isInitialized = true;
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
