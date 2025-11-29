using Core.Configuration;
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
        private Vector2 _fieldCenter = Vector2.zero;
        private Vector2 _fieldSize;
        private SceneBorderFactory _sceneBorderFactory;

        public Bounds Bounds => new Bounds(_fieldCenter, _fieldSize);

        [Inject]
        public void Construct(JsonConfigProvider configProvider, SceneBorderFactory sceneBorderFactory)
        {
            _environmentSettings = configProvider.EnvironmentSettingsRef;

            float fieldWidth = Math.Max(_environmentSettings.GameFieldWidth, _environmentSettings.MinimalFieldWidth);
            float fieldHeight = Math.Max(_environmentSettings.GameFieldHeight, _environmentSettings.MinimalFieldHeight);

            _fieldSize = new Vector2(fieldWidth, fieldHeight);

            _sceneBorderFactory = sceneBorderFactory;

            CreateSceneBorders();
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
