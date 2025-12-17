using Core.Configuration;
using Core.Systems;
using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using Core.Components;
using Core.Physics;
using Enemies.Logic;
using Enemies.Signals;
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

            _poolAccessProvider.SetPool(_objectPool);
        }

        private void BindPoolAccessProvider()
        {
            _poolAccessProvider = new PoolAccessProvider();
            Container.Bind<PoolAccessProvider>().FromInstance(_poolAccessProvider).AsSingle();
        }


        private void BindLogicSystems()
        {
            Container.BindInterfacesTo<CollisionHandler>().AsCached();
            Container.BindInterfacesAndSelfTo<HealthSystem>().FromNew().AsTransient();
            Container.Bind<CustomPhysics>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<BigAsteroidLogic>().FromNew().AsTransient();
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

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<EnemySpawnedSignal>();
            Container.DeclareSignal<EnemyDestroyedSignal>();
        }
    }
}
