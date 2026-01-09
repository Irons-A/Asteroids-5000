using Gameplay.Environment;
using Gameplay.Environment.Systems;
using UnityEngine;
using Zenject;

namespace Gameplay.Infrastructure
{
    public class EnvironmentInstaller : MonoInstaller
    {
        [SerializeField] private SceneBorder _sceneBorderPrefab;

        public override void InstallBindings()
        {
            Container.Bind<SceneBorder>().FromInstance(_sceneBorderPrefab).AsTransient();
            Container.Bind<SceneBorderFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnemySpawner>().AsSingle();
            Container.BindInterfacesAndSelfTo<EnvironmentLogic>().AsSingle();
        }
    }
}
