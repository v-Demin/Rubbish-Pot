public class MixingCondition : AbstractCondition, IMixingHandler
{
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;

    public void HandleBeingMixed(IReactionPart target)
    {
        EventBus.RaiseEvent<IAmbivalentConditionReachedHandler>(h => h.HandleConditionReached());
        EventBus.RaiseEvent<ISoloConditionReachedHandler>(h => h.HandleConditionReached(target));
    }
}
