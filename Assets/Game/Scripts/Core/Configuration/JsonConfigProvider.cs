using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Saves;
using UnityEngine;
using Zenject;

namespace Core.Configuration
{
    public class JsonConfigProvider : IInitializable
    {
        private const string UserInputConfigPath = "Configs/UserInputConfig";
        private const string PlayerConfigPath = "Configs/PlayerConfig";
        private const string EnvironmentConfigPath = "Configs/EnvironmentConfig";
        private const string EnemyConfigPath = "Configs/EnemyConfig";

        private const string SaveDataFileName = "saveData.json";
        
        private UserInputSettings _userInputSettings;
        private PlayerSettings _playerSettings;
        private EnvironmentSettings _environmentSettings;
        private EnemySettings _enemySettings;
        
        private PlayerSaveData _playerSaveData;
        private string _saveDataFilePath;

        public bool IsInitialized { get; private set; } = false;

        public int HighScore => _playerSaveData?.HighScore ?? 0;
        
        [Inject]
        private void Construct()
        {
            EnsureInitialized();
        }
        
        public void Initialize()
        {
            _saveDataFilePath = Path.Combine(Application.persistentDataPath, SaveDataFileName );
            
            LoadHighScore();
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
        
        public void TryUpdatingHighScore(int newScore)
        {
            EnsureInitialized();
            
            PlayerSaveData newSaveData = _playerSaveData.WithUpdatedHighScore(newScore);
            
            if (ReferenceEquals(newSaveData, _playerSaveData) == false)
            {
                _playerSaveData = newSaveData;
                
                SaveHighScore();
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
        
        private void LoadHighScore()
        {
            if (File.Exists(_saveDataFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_saveDataFilePath);
                    _playerSaveData = JsonConvert.DeserializeObject<PlayerSaveData>(json);
                
                    if (_playerSaveData == null)
                    {
                        _playerSaveData = PlayerSaveData.CreateDefault();
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to load high score: {ex.Message}");
                    
                    _playerSaveData = PlayerSaveData.CreateDefault();
                }
            }
            else
            {
                _playerSaveData = PlayerSaveData.CreateDefault();
                
                SaveHighScore();
            }
        }
        
        private void SaveHighScore()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_playerSaveData, Formatting.Indented);
                
                File.WriteAllText(_saveDataFilePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save high score: {ex.Message}");
            }
        }
    }
}
