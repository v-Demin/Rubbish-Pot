using Zenject;

public class InventoryInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<Inventory>()
            .FromNew()
            .AsSingle();
    }
}
