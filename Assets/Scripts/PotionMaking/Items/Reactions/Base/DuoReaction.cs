[System.Serializable]
public abstract class DuoReaction : AbstractReaction, IDuoConditionReachedHandler
{
    public sealed override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Duo;

    public void HandleConditionReached(IReactionPart target, IReactionPart other)
    {
        Execute(target, other);
    }

    public abstract void Execute(IReactionPart target, IReactionPart other);
}
