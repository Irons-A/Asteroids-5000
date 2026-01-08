using Core.Configuration;
using Gameplay.Environment.Systems;
using System;
using Core.Configuration.Environment;
using UnityEngine;
using Zenject;

namespace Gameplay.Environment
{
    public class EnvironmentLogic : IInitializable
    {
        private readonly SceneBorderFactory _sceneBorderFactory;
        private readonly EnemySpawner _enemySpawner;

        private readonly Vector2 _fieldCenter = Vector2.zero;
        private readonly Vector2 _fieldSize;

        public Bounds Bounds => new Bounds(_fieldCenter, _fieldSize);
        
        public EnvironmentLogic(JsonConfigProvider configProvider, SceneBorderFactory sceneBorderFactory,
            EnemySpawner enemySpawner)
        {
            GameFieldSettings gameFieldSettings = configProvider.GameFieldSettingsRef;

            float fieldWidth = Math.Max(gameFieldSettings.GameFieldWidth, gameFieldSettings.MinimalFieldWidth);
            float fieldHeight = Math.Max(gameFieldSettings.GameFieldHeight, gameFieldSettings.MinimalFieldHeight);
            _fieldSize = new Vector2(fieldWidth, fieldHeight);

            _enemySpawner = enemySpawner;
            _sceneBorderFactory = sceneBorderFactory;
        }

        public void Initialize()
        {
            CreateSceneBorders();
            
            _enemySpawner.SetGameFieldBounds(Bounds);
        }

        private void CreateSceneBorders()
        {
            CreateBorder(BorderSide.Left);
            CreateBorder(BorderSide.Right);
            CreateBorder(BorderSide.Top);
            CreateBorder(BorderSide.Bottom);
        }

        private void CreateBorder(BorderSide borderSide)
        {
            SceneBorder border = _sceneBorderFactory.CreateBorder();

            border.Configure(borderSide, _fieldSize.x, _fieldSize.y);
        }
    }
}
