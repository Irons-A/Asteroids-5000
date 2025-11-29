using Gameplay.Environment;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
            Container.Bind<SceneBorderFactory>().FromNew().AsSingle();
            Container.Bind<EnvironmentLogic>().FromNew().AsSingle();
        }
    }
}
