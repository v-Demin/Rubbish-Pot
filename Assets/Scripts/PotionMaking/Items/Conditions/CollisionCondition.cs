using UnityEngine;

public class CollisionCondition : AbstractCondition, ICollisionHandler
{
    [SerializeField] private ReactionConfig _collisionConfigToReact;
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Duo;
        
    public void HandleCollision(ReactionComponent target, ReactionComponent other)
    {
        if (_collisionConfigToReact == other.Config)
        {
            ConditionReached(target);
        }
    }
}