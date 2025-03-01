[System.Serializable]
public abstract class AbstractCondition : ISubscriber, IConnectinable
{
    protected EventBus EventBus;
    public abstract IConnectinable.ConnectionType Connection { get; }

    public void Init(EventBus eventBus)
    {
        EventBus = eventBus;
    }
}
