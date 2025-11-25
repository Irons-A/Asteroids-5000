using Core.Physics;
using Player.Logic;
using Player.UserInput.Strategies;
using Player.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Insfrastructure
{
    public class GameSceneInstaller : MonoInstaller
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
            Container.Bind<KeyboardMouseInputStrategy>().FromNew().AsSingle();
            Container.Bind<GamepadInputStrategy>().FromNew().AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerModel>().FromNew().AsSingle();

            GameObject playerInstance = Container.InstantiatePrefab(_playerPrefab,
                _playerSpawnPoint?.position ?? Vector3.zero, Quaternion.identity, null);

            Container.Bind<PlayerView>().FromComponentOn(playerInstance).AsSingle();
        }
    }
}
