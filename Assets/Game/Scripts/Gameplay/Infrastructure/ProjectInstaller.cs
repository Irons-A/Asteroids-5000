using Core.Configuration;
using Core.Systems;
using Core.Systems.ObjectPools;
using Advertisement;
using Analytics;
using Core.Logic;
using Core.Physics;
using Core.Saves;
using Core.Signal;
using Core.Signals;
using Core.UserInput;
using Enemies.Logic;
using Enemies.Signals;
using Gameplay.Signals;
using Gameplay.Systems;
using Player.Signals;
using Player.UserInput;
using Player.UserInput.Strategies;
using UI.PlayerMVVM;
using UI.Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private UniversalObjectPool _objectPoolPrefab;
        [SerializeField] private PoolableObjectRegistry _poolableObjectRegistry;

        private PoolAccessProvider _poolAccessProvider;
        private UniversalObjectPool _objectPool;

        public override void InstallBindings()
        {
            Container.Bind<JsonConfigProvider>().FromNew().AsSingle().NonLazy();
            Container.Bind<SaveSystem>().FromNew().AsSingle().NonLazy();

            BindAnalytics();
            BindAdvertisement();
            InstallSignals();
            BindPoolAccessProvider();
            
            Container.Bind<ParticleService>().FromNew().AsSingle().NonLazy();
            
            BindLogicSystems();
            BindObjectPool();
            BindUserInput();
            BindGameUI();
            

            _poolAccessProvider.SetPool(_objectPool);
        }

        private void BindAnalytics()
        {
            Container.Bind<AnalyticsLogger>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AnalyticsLogger>().AsSingle().NonLazy();
        }

        private void BindAdvertisement()
        {
            Container.BindInterfacesAndSelfTo<BannerDisplayer>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InterstitialDisplayer>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AdvertisementInitializer>().AsSingle().NonLazy();
        }

        private void BindPoolAccessProvider()
        {
            _poolAccessProvider = new PoolAccessProvider();
            Container.Bind<PoolAccessProvider>().FromInstance(_poolAccessProvider).AsSingle();
        }

        private void BindLogicSystems()
        {
            Container.BindInterfacesAndSelfTo<ProjectileLogic>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<HealthSystem>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<CustomPhysics>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<BigAsteroidLogic>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<SmallAsteroidLogic>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<UFOLogic>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<EnemyShootingSystem>().FromNew().AsTransient();
        }
        
        private void BindObjectPool()
        {
            Container.Bind<PoolableObjectRegistry>().FromInstance(_poolableObjectRegistry).AsSingle().NonLazy();
            Container.Bind<PoolableObjectFactory>().FromNew().AsSingle().NonLazy();
            _objectPool = CreateUniversalObjectPool();
            Container.Bind<UniversalObjectPool>().FromInstance(_objectPool).AsSingle().NonLazy();
        }

        private UniversalObjectPool CreateUniversalObjectPool()
        {
            UniversalObjectPool objectPool = Container.InstantiatePrefabForComponent<UniversalObjectPool>(
                _objectPoolPrefab, Vector3.zero, Quaternion.identity, null);

            DontDestroyOnLoad(objectPool.gameObject);

            return objectPool;
        }

        private void BindUserInput()
        {
            Container.Bind<KeyboardMouseInputStrategy>().AsSingle();
            Container.Bind<GamepadInputStrategy>().AsSingle();
            Container.Bind<MobileInputMediator>().AsSingle();
            Container.Bind<MobileInputStrategy>().AsSingle();
            Container.BindInterfacesAndSelfTo<InputDetector>().FromNew().AsSingle().NonLazy();
        }

        private void BindGameUI()
        {
            Container.Bind<PlayerUIModel>().AsSingle().NonLazy();
            Container.Bind<PlayerUIViewModel>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ScoreCounter>().AsSingle().NonLazy();
        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<DespawnAllSignal>();
            Container.DeclareSignal<EndGameSignal>();
            Container.DeclareSignal<PauseGameSignal>();

            Container.DeclareSignal<EnemySpawnedSignal>();
            Container.DeclareSignal<EnemyDestroyedSignal>();
            Container.DeclareSignal<ResetEnemyCountSignal>();

            Container.DeclareSignal<ResetScoreSignal>();
            Container.DeclareSignal<StartEnemySpawningSignal>();
            Container.DeclareSignal<StopEnemySpawningSignal>();

            Container.DeclareSignal<DisablePlayerSignal>();
            Container.DeclareSignal<ResetPlayerSignal>();
            
            Container.DeclareSignal<ContinueGameSignal>();
            Container.DeclareSignal<ExitGameSignal>();
            Container.DeclareSignal<GoToMenuSignal>();
            Container.DeclareSignal<RestartGameSignal>();
            Container.DeclareSignal<StartGameSignal>();
        }
    }
}
