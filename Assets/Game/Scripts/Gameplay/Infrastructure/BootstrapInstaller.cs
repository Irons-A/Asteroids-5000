using Zenject;

namespace Gameplay.Infrastructure
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().To<BootstrapLoader>().FromComponentInHierarchy().AsSingle();
        }
    }
}
