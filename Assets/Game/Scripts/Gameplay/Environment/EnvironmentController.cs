using Core.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Environment
{
    public class EnvironmentController
    {
        private EnvironmentSettings _environmentSettings;
        private Vector2 _fieldCenter = Vector2.zero;
        private Vector2 _fieldSize;

        public Bounds Bounds => new Bounds(_fieldCenter, _fieldSize);

        [Inject]
        public void Construct(JsonConfigProvider configProvider)
        {
            _environmentSettings = configProvider.EnvironmentSettingsRef;

            _fieldSize = new Vector2(_environmentSettings.GameFieldWidth, _environmentSettings.GameFieldHeight);
        }
    }
}
