using UnityEngine;

public class CollisionCondition : AbstractCondition, ICollisionHandler
{
    [SerializeField] private ReactionConfig _collisionConfigToReact;
        
    public void HandleCollision(ReactionComponent target, ReactionComponent other)
    {
        if (_collisionConfigToReact == other.Config)
        {
            ConditionReached(target);
        }
    }
}