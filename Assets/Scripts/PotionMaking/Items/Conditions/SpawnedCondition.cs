public class SpawnedCondition : AbstractCondition, ISpawnHandler
{
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;

    public void HandleSpawn(IReactionPart target)
    {
        EventBus.RaiseEvent<IAmbivalentConditionReachedHandler>(h => h.HandleConditionReached());
        EventBus.RaiseEvent<ISoloConditionReachedHandler>(h => h.HandleConditionReached(target));
    }
}
