using Core.Physics;
using Player.Logic;
using Player.UserInput;
using Player.UserInput.Strategies;
using Player.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Insfrastructure
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;

        public override void InstallBindings()
        {
            Container.Bind<CustomPhysics>().FromNew().AsTransient();
            Container.Bind<PlayerView>().FromMethod(CreatePlayerView).AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerModel>().AsSingle();
            Container.Bind<KeyboardMouseInputStrategy>().AsSingle();
            Container.Bind<GamepadInputStrategy>().AsSingle();
        }

        private PlayerView CreatePlayerView(InjectContext context)
        {
            GameObject playerInstance = Instantiate(_playerPrefab,
                _playerSpawnPoint?.position ?? Vector3.zero, Quaternion.identity);

            PlayerView playerView = playerInstance.GetComponent<PlayerView>();

            if (playerView == null)
            {
                playerView = playerInstance.AddComponent<PlayerView>();
            }    

            InputDetector inputDetector = playerInstance.AddComponent<InputDetector>();
            Container.Inject(inputDetector);

            return playerView;
        }
    }
}
