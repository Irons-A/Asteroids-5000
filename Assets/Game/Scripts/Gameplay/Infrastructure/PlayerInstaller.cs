using Player.Logic;
using Player.Logic.Weapons;
using Player.Presentation;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerPresentation _playerPrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InvulnerabilityLogic>().AsTransient();
            Container.BindInterfacesAndSelfTo<UncontrollabilityLogic>().AsTransient();
            Container.Bind<PlayerWeaponConfig>().AsTransient();
            Container.Bind<PlayerAmmoSubsystem>().AsTransient();
            Container.Bind<PlayerReloadingSubsystem>().AsTransient();
            Container.BindInterfacesAndSelfTo<PlayerShootingSubsystem>().AsTransient();
            Container.BindInterfacesAndSelfTo<UniversalPlayerWeaponSystem>().AsTransient();
            Container.BindInterfacesAndSelfTo<PlayerMovementLogic>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthLogic>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerWeaponsLogic>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerLogic>().AsSingle().NonLazy();
            
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
