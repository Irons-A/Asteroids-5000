using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Environment
{
    public class SceneBorderFactory
    {
        private readonly SceneBorder _borderPrefab;
        private readonly DiContainer _container;

        private Transform _borderParent;
        private Vector3 _spawnPosition = new Vector3(0, -100, 0);

        public SceneBorderFactory(SceneBorder borderPrefab, DiContainer container)
        {
            _borderPrefab = borderPrefab;
            _container = container;

            GameObject parent = new GameObject("SceneBorders");
            _borderParent = parent.transform;
        }

        public SceneBorder CreateBorder()
        {
            return _container.InstantiatePrefabForComponent<SceneBorder>(
                _borderPrefab, _spawnPosition, Quaternion.identity, _borderParent);
        }
    }
}
