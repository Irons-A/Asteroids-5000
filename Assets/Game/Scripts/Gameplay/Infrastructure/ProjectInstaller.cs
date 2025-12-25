using Core.Configuration;
using Core.Systems;
using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Logic;
using Core.Physics;
using Core.Signal;
using Enemies.Logic;
using Enemies.Signals;
using Gameplay.Signals;
using Gameplay.Systems;
using UI;
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
            Container.BindInterfacesAndSelfTo<JsonConfigProvider>().FromNew().AsSingle().NonLazy();
            
            InstallSignals();
            BindPoolAccessProvider();
            BindLogicSystems();
            BindObjectPool();
            BindGameUI();

            _poolAccessProvider.SetPool(_objectPool);
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

            Container.DeclareSignal<EnemySpawnedSignal>();
            Container.DeclareSignal<EnemyDestroyedSignal>();
            Container.DeclareSignal<ResetEnemyCountSignal>();

            Container.DeclareSignal<EndGameSignal>();
            
            Container.DeclareSignal<ContinueGameSignal>();
            Container.DeclareSignal<ExitGameSignal>();
            Container.DeclareSignal<GoToMenuSignal>();
            Container.DeclareSignal<PauseGameSignal>();
            Container.DeclareSignal<RestartGameSignal>();
            Container.DeclareSignal<StartGameSignal>();
        }
    }
}
