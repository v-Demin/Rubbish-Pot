using System;

public abstract class AbstractCondition : ISubscriber, IConnectinable
{
    public event Action<ReactionComponent> OnConditionReached;
    public abstract IConnectinable.ConnectionType Connection { get; }

    protected void ConditionReached(ReactionComponent target)
    {
        OnConditionReached?.Invoke(target);
    }
}
