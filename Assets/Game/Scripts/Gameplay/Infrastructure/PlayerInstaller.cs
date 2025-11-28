using Core.Physics;
using Player.Logic;
using Player.Presentation;
using Player.UserInput;
using Player.UserInput.Strategies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;

        public override void InstallBindings()
        {
            Container.Bind<CustomPhysics>().FromNew().AsTransient();
            Container.Bind<PlayerPresentation>().FromMethod(CreatePlayerPresentation).AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerLogic>().AsSingle();
            Container.Bind<KeyboardMouseInputStrategy>().AsSingle();
            Container.Bind<GamepadInputStrategy>().AsSingle();
        }

        private PlayerPresentation CreatePlayerPresentation(InjectContext context)
        {
            GameObject playerInstance = Instantiate(_playerPrefab,
                _playerSpawnPoint?.position ?? Vector3.zero, Quaternion.identity);

            PlayerPresentation playerView = playerInstance.GetComponent<PlayerPresentation>();

            if (playerView == null)
            {
                playerView = playerInstance.AddComponent<PlayerPresentation>();
            }    

            InputDetector inputDetector = playerInstance.AddComponent<InputDetector>();
            Container.Inject(inputDetector);

            return playerView;
        }
    }
}
