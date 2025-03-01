[System.Serializable]
public class DestroyReaction : AbstractReaction
{
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Ambivalent;

    public override void Execute(IReactionPart target)
    {
        target.Destroy();
    }
}
