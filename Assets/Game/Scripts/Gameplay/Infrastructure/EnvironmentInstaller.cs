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
        public override void InstallBindings()
        {
            Container.Bind<EnvironmentController>().FromNew().AsSingle();
        }
    }
}
