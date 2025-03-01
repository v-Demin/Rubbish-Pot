[System.Serializable]
public abstract class AmbivalentReaction : AbstractReaction, IAmbivalentConditionReachedHandler
{
    public sealed override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Ambivalent;

    public void HandleConditionReached(IReactionPart target, IReactionPart other)
    {
        Execute();
    }

    public void HandleConditionReached(IReactionPart target)
    {
        Execute();
    }

    public void HandleConditionReached()
    {
        Execute();
    }

    public abstract void Execute();
}
