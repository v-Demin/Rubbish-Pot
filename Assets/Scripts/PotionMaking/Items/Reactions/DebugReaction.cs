using UnityEngine;

public class DebugReaction : AbstractReaction
{
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Ambivalent;

    [SerializeField] private string _message;

    public override void Execute(IReactionPart target)
    {
        _message.Log(Color.green);
    }
}