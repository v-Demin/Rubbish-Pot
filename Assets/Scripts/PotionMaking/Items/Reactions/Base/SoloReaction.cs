using UnityEngine;

[System.Serializable]
public abstract class SoloReaction : AbstractReaction, ISoloConditionReachedHandler
{
    [SerializeField] private Target _target;
    
    public sealed override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;
    
    private bool CheckIfConnectionWithConditionIsDuo()
    {
        return ConnectionWithCondition == IConnectinable.ConnectionType.Duo;
    }
    
    public void HandleConditionReached(IReactionPart target, IReactionPart other)
    {
        if (CheckIfConnectionWithConditionIsDuo() && _target == Target.Other)
        {
            Execute(other);
        }
        else
        {
            Execute(target);
        }
    }

    public void HandleConditionReached(IReactionPart target)
    {
        Execute(target);
    }

    public abstract void Execute(IReactionPart target);
    
    private enum Target
    {
        Owner = 0,
        Other = 10
    }
}
