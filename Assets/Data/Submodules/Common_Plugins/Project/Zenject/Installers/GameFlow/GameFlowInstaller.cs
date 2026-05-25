using RubbishPot.Core;
using Zenject;

namespace RubbishPot.Plugins
{
    public class GameFlowInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<GameFlowService>()
                .FromNew()
                .AsSingle();
        }
    }
}
