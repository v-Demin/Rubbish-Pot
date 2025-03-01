public class MixingCondition : AbstractCondition, IMixingHandler
{
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;

    public void HandleBeingMixed(ReactionComponent component)
    {
        ConditionReached(component);
    }

}
