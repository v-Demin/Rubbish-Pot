using UnityEngine;

public class CollisionCondition : AbstractCondition, ICollisionHandler
{
    [SerializeField] private ReactionConfig _collisionConfigToReact;
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Duo;
        
    public void HandleCollision(ReactionComponent target, ReactionComponent other)
    {
        if (_collisionConfigToReact != other.Config) return;
        
        EventBus.RaiseEvent<IAmbivalentConditionReachedHandler>(h => h.HandleConditionReached());
        EventBus.RaiseEvent<ISoloConditionReachedHandler>(h => h.HandleConditionReached(target));
        EventBus.RaiseEvent<IDuoConditionReachedHandler>(h => h.HandleConditionReached(target, other));
    }
}