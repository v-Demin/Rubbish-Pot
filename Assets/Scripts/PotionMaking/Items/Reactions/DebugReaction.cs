using UnityEngine;

public class DebugReaction : AbstractReaction
{
    public override void Execute(IReactionPart target)
    {
        "Debug successful, condition works fine".Log(Color.green);
    }
}