using Core.Physics;
using Player.Logic;
using Player.Logic.Weapons;
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
        [SerializeField] private PlayerPresentation _playerPrefab;
        [SerializeField] private Transform _playerSpawnPoint;

        public override void InstallBindings()
        {
            Container.Bind<PlayerPresentation>().FromMethod(CreatePlayerPresentation).AsSingle();
            Container.BindInterfacesAndSelfTo<InvulnerabilityLogic>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<UncontrollabilityLogic>().FromNew().AsTransient();
            Container.Bind<PlayerWeaponConfig>().FromNew().AsTransient();
            Container.Bind<PlayerAmmoSubsystem>().FromNew().AsTransient();
            Container.Bind<PlayerReloadingSubsystem>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<PlayerShootingSubsystem>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<UniversalPlayerWeaponSystem>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<PlayerLogic>().AsSingle();
            Container.Bind<KeyboardMouseInputStrategy>().AsSingle();
            Container.Bind<GamepadInputStrategy>().AsSingle();
        }

        private PlayerPresentation CreatePlayerPresentation()
        {
            PlayerPresentation playerInstance = Instantiate(_playerPrefab,
                _playerSpawnPoint?.position ?? Vector3.zero, Quaternion.identity);

            InputDetector inputDetector = playerInstance.gameObject.AddComponent<InputDetector>();
            Container.Inject(inputDetector);

            return playerInstance;
        }
    }
}
