using UnityEngine;

public class MixingCondition : AbstractCondition, IMixingHandler
{
    public void HandleBeingMixed(ReactionComponent component)
    {
        ConditionReached(component);
    }
}
