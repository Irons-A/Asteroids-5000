using UnityEngine;
using Zenject;

namespace Gameplay.Environment
{
    public class SceneBorderFactory
    {
        private readonly SceneBorder _borderPrefab;
        private readonly DiContainer _container;

        private readonly Transform _borderParent;
        private readonly Vector3 _spawnPosition = new Vector3(0, -100, 0);

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
