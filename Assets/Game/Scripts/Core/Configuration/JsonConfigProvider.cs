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
        public const string UserInputConfigPath = "Configs/UserInputConfig";
        public const string PlayerConfigPath = "Configs/PlayerConfig";
        public const string EnvironmentConfigPath = "Configs/EnvironmentConfig";
        public const string EnemyConfigPath = "Configs/EnemyConfig";

        public const string HighScoreFileName = "highscore.json";
        
        private UserInputSettings _userInputSettings;
        private PlayerSettings _playerSettings;
        private EnvironmentSettings _environmentSettings;
        private EnemySettings _enemySettings;
        
        private HighScoreData _highScoreData;
        private string _highScoreFilePath;
        
        private bool _isInitialized = false;
        
        public int HighScore => _highScoreData?.HighScore ?? 0;
        
        [Inject]
        private void Construct()
        {
            EnsureInitialized();
        }
        
        public void Initialize()
        {
            _highScoreFilePath = Path.Combine(
                Application.persistentDataPath, 
                HighScoreFileName
            );
            
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
        
            if (newScore > HighScore)
            {
                _highScoreData.HighScore = newScore;
                SaveHighScore();
            
                Debug.Log($"New high score: {newScore}");
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
        
        private void LoadHighScore()
        {
            if (File.Exists(_highScoreFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_highScoreFilePath);
                    _highScoreData = JsonConvert.DeserializeObject<HighScoreData>(json);
                
                    if (_highScoreData == null)
                    {
                        _highScoreData = new HighScoreData();
                    }
                
                    Debug.Log($"Loaded high score: {_highScoreData.HighScore}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to load high score: {ex.Message}");
                    _highScoreData = new HighScoreData();
                }
            }
            else
            {
                _highScoreData = new HighScoreData();
                SaveHighScore();
                Debug.Log("High score file created with default value");
            }
        }
        
        private void SaveHighScore()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_highScoreData, Formatting.Indented);
                File.WriteAllText(_highScoreFilePath, json);
            
                Debug.Log($"High score saved: {_highScoreData.HighScore}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save high score: {ex.Message}");
            }
        }
    }
}
