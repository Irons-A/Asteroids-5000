using Core.Configuration;
using Core.Systems;
using Core.Systems.ObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private UniversalObjectPool _objectPool;
        [SerializeField] private PoolableObjectRegistry _poolableObjectRegistry;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<JsonConfigProvider>().FromNew().AsSingle().NonLazy();

            Container.Bind<PoolableObjectRegistry>().FromInstance(_poolableObjectRegistry).AsSingle().NonLazy();
            Container.Bind<PoolableObjectFactory>().FromNew().AsSingle().NonLazy();
            Container.Bind<UniversalObjectPool>().FromMethod(CreateUniversalObjectPool).AsSingle().NonLazy();
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
