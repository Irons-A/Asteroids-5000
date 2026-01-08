using Core.Configuration.Enemies;
using Core.Configuration.Environment;
using Core.Configuration.Player;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Configuration
{
    public class JsonConfigProvider
    {
        private const string UserInputConfigPath = "Configs/Player/UserInputConfig";
        private const string PlayerShipConfigPath = "Configs/Player/PlayerShipConfig";
        private const string PlayerWeaponsConfigPath = "Configs/Player/PlayerWeaponsConfig";
        
        private const string GameFieldConfigPath = "Configs/Environment/GameFieldConfig";
        private const string EnemySpawnConfigPath = "Configs/Environment/EnemySpawnConfig";
        
        private const string BigAsteroidConfigPath = "Configs/Enemies/BigAsteroidConfig";
        private const string SmallAsteroidConfigPath = "Configs/Enemies/SmallAsteroidConfig";
        private const string UFOConfigPath = "Configs/Enemies/UFOConfig";
        private const string EnemyRewardsConfigPath = "Configs/Enemies/EnemyRewardsConfig";
        
        private UserInputSettings _userInputSettings;
        private PlayerShipSettings _playerShipSettings;
        private PlayerWeaponsSettings _playerWeaponsSettings;
        
        private GameFieldSettings _gameFieldSettings;
        private EnemySpawnSettings _enemySpawnSettings;
        
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

        public PlayerShipSettings PlayerShipSettingsRef
        { 
            get
            {
                EnsureInitialized();
                return _playerShipSettings;
            }
        }
        
        public PlayerWeaponsSettings PlayerWeaponsSettingsRef
        { 
            get
            {
                EnsureInitialized();
                return _playerWeaponsSettings;
            }
        }
        
        public GameFieldSettings GameFieldSettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _gameFieldSettings;
            }
        }
        
        public EnemySpawnSettings EnemySpawnSettingsRef 
        { 
            get 
            {
                EnsureInitialized();
                return _enemySpawnSettings;
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
            _playerShipSettings = LoadConfig<PlayerShipSettings>(PlayerShipConfigPath);
            _playerWeaponsSettings = LoadConfig<PlayerWeaponsSettings>(PlayerWeaponsConfigPath);
            
            _gameFieldSettings = LoadConfig<GameFieldSettings>(GameFieldConfigPath);
            _enemySpawnSettings = LoadConfig<EnemySpawnSettings>(EnemySpawnConfigPath);
            
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
