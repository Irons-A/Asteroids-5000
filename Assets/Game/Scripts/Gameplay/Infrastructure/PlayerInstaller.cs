using Core.Physics;
using Player.Logic;
using Player.Logic.Weapons;
using Player.Presentation;
using Player.UserInput;
using Player.UserInput.Strategies;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerPresentation _playerPrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InvulnerabilityLogic>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<UncontrollabilityLogic>().FromNew().AsTransient();
            Container.Bind<PlayerWeaponConfig>().FromNew().AsTransient();
            Container.Bind<PlayerAmmoSubsystem>().FromNew().AsTransient();
            Container.Bind<PlayerReloadingSubsystem>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<PlayerShootingSubsystem>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<UniversalPlayerWeaponSystem>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<PlayerLogic>().FromNew().AsSingle().NonLazy();
            
            Container.BindInterfacesAndSelfTo<PlayerPresentation>() .FromMethod(CreatePlayerWithTransform) .AsSingle();
        }

        private PlayerPresentation CreatePlayerWithTransform(InjectContext ctx)
        {
            GameObject playerObj = ctx.Container.InstantiatePrefab( _playerPrefab,  Vector3.zero,    
                Quaternion.identity, null);
            
            return playerObj.GetComponent<PlayerPresentation>();
        }
    }
}
