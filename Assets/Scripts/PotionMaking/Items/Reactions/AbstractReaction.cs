[System.Serializable]
public abstract class AbstractReaction : IConnectinable
{
    public abstract IConnectinable.ConnectionType Connection { get; }
    public abstract void Execute(IReactionPart target);
}