using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Core.Saves
{
    public class SaveSystem
    {
        private const string SaveDataFileName = "saveData.json";
        
        private PlayerSaveData _playerSaveData;
        private string _saveDataFilePath;
        
        public int HighScore => _playerSaveData?.HighScore ?? 0;

        public SaveSystem()
        {
            _saveDataFilePath = Path.Combine(Application.persistentDataPath, SaveDataFileName );
            
            LoadHighScore();
        }
        
        public void TryUpdatingHighScore(int newScore)
        {
            PlayerSaveData newSaveData = _playerSaveData.CreateWithUpdatedHighScore(newScore);
            
            if (ReferenceEquals(newSaveData, _playerSaveData) == false)
            {
                _playerSaveData = newSaveData;
                
                SaveHighScore();
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
