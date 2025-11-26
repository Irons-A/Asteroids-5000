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
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;

        public override void InstallBindings()
        {
            Container.Bind<CustomPhysics>().FromNew().AsTransient();

            BindPlayer();
        }

        private void BindPlayer()
        {
            GameObject playerInstance = Container.InstantiatePrefab(_playerPrefab,
                _playerSpawnPoint?.position ?? Vector3.zero, Quaternion.identity, null);

            Container.Bind<PlayerView>().FromComponentOn(playerInstance).AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerModel>().FromNew().AsSingle();

            Container.Bind<KeyboardMouseInputStrategy>().FromNew().AsSingle();
            Container.Bind<GamepadInputStrategy>().FromNew().AsSingle();

            InputDetector inputDetector = playerInstance.AddComponent<InputDetector>();
            Container.Inject(inputDetector);
        }
    }
}
