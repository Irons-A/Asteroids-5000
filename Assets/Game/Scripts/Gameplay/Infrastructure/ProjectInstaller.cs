using Core.Configuration;
using Core.Systems;
using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using Core.Physics;
using Enemies.Logic;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private UniversalObjectPool _objectPool;
        [SerializeField] private PoolAccessProvider _poolAccessProvider;
        [SerializeField] private PoolableObjectRegistry _poolableObjectRegistry;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<JsonConfigProvider>().FromNew().AsSingle().NonLazy();
            
            Container.Bind<PoolAccessProvider>().FromMethod(CreatePoolAccessProvider).AsSingle().NonLazy();

            Container.Bind<HealthSystem>().FromNew().AsTransient();
            Container.Bind<CustomPhysics>().FromNew().AsTransient();
            Container.BindInterfacesAndSelfTo<BigAsteroidLogic>().FromNew().AsTransient();

            Container.Bind<PoolableObjectRegistry>().FromInstance(_poolableObjectRegistry).AsSingle().NonLazy();
            Container.Bind<PoolableObjectFactory>().FromNew().AsSingle().NonLazy();
            Container.Bind<UniversalObjectPool>().FromMethod(CreateUniversalObjectPool).AsSingle().NonLazy();
        }

        private PoolAccessProvider CreatePoolAccessProvider()
        {
            PoolAccessProvider poolAccessesProvider = Container.InstantiatePrefabForComponent<PoolAccessProvider>(
                _poolAccessProvider, Vector3.zero, Quaternion.identity, null);
            
            DontDestroyOnLoad(poolAccessesProvider.gameObject);

            return poolAccessesProvider;
        }

        private UniversalObjectPool CreateUniversalObjectPool()
        {
            UniversalObjectPool objectPool = Container.InstantiatePrefabForComponent<UniversalObjectPool>(
                _objectPool, Vector3.zero, Quaternion.identity, null);

            DontDestroyOnLoad(objectPool.gameObject);

            return objectPool;
        }
    }
}
