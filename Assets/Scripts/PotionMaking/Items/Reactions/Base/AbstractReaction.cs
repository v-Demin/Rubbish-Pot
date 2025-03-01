[System.Serializable]
public abstract class AbstractReaction : ISubscriber, IConnectinable
{
    public abstract IConnectinable.ConnectionType Connection { get; }
    
    protected IConnectinable.ConnectionType ConnectionWithCondition;

    public void InformAboutConnection(IConnectinable.ConnectionType connectionType)
    {
        ConnectionWithCondition = connectionType;
    }
}