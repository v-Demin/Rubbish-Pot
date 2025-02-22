using UnityEngine;

public class DebugReaction : AbstractReaction
{
    public override void Execute(ReactionComponent target)
    {
        "Debug successful, condition works fine".Log(Color.green);
    }
}