using Core.Configuration.Enemies;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class JsonConfigProvider
    {
        private const string UserInputConfigPath = "Configs/UserInputConfig";
        private const string PlayerConfigPath = "Configs/PlayerConfig";
        private const string EnvironmentConfigPath = "Configs/EnvironmentConfig";
        
        private const string BigAsteroidConfigPath = "Configs/Enemies/BigAsteroidConfig";
        private const string SmallAsteroidConfigPath = "Configs/Enemies/SmallAsteroidConfig";
        private const string UFOConfigPath = "Configs/Enemies/UFOConfig";
        private const string EnemyRewardsConfigPath = "Configs/Enemies/EnemyRewardsConfig";
        
        private UserInputSettings _userInputSettings;
        private PlayerSettings _playerSettings;
        private EnvironmentSettings _environmentSettings;
        
        private BigAsteroidSettings _bigAsteroidSettings;
        private SmallAsteroidSettings _smallAsteroidSettings;
        private UFOSettings _uFOSettings;
        private EnemyRewardsSettings _enemyRewardsSettings;
        
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

        public BigAsteroidSettings BigAsteroidSettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _bigAsteroidSettings;
            }
        }
        
        public SmallAsteroidSettings SmallAsteroidSettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _smallAsteroidSettings;
            }
        }
        
        public UFOSettings UFOSettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _uFOSettings;
            }
        }
        
        public EnemyRewardsSettings enemyRewardsSettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _enemyRewardsSettings;
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
            
            _bigAsteroidSettings = LoadConfig<BigAsteroidSettings>(BigAsteroidConfigPath);
            _smallAsteroidSettings = LoadConfig<SmallAsteroidSettings>(SmallAsteroidConfigPath);
            _uFOSettings = LoadConfig<UFOSettings>(UFOConfigPath);
            _enemyRewardsSettings = LoadConfig<EnemyRewardsSettings>(EnemyRewardsConfigPath);
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
