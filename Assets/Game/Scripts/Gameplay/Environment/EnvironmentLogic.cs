using Core.Configuration;
using Gameplay.Environment.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Environment
{
    public class EnvironmentLogic
    {
        private EnvironmentSettings _environmentSettings;
        private SceneBorderFactory _sceneBorderFactory;
        private EnemySpawner _enemySpawner;

        private Vector2 _fieldCenter = Vector2.zero;
        private Vector2 _fieldSize;

        public Bounds Bounds => new Bounds(_fieldCenter, _fieldSize);

        [Inject]
        private void Construct(JsonConfigProvider configProvider, SceneBorderFactory sceneBorderFactory,
            EnemySpawner enemySpawner)
        {
            _environmentSettings = configProvider.EnvironmentSettingsRef;

            float fieldWidth = Math.Max(_environmentSettings.GameFieldWidth, _environmentSettings.MinimalFieldWidth);
            float fieldHeight = Math.Max(_environmentSettings.GameFieldHeight, _environmentSettings.MinimalFieldHeight);

            _fieldSize = new Vector2(fieldWidth, fieldHeight);

            _enemySpawner = enemySpawner;
            _sceneBorderFactory = sceneBorderFactory;

            CreateSceneBorders();
            _enemySpawner.StartEnemySpawning(Bounds);
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

            border.Initialize(borderSide, _fieldSize.x, _fieldSize.y);
        }
    }
}
