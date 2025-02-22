using System;

public abstract class AbstractCondition : ISubscriber
{
    public event Action<ReactionComponent> OnConditionReached;

    protected void ConditionReached(ReactionComponent target)
    {
        OnConditionReached?.Invoke(target);
    }
}
