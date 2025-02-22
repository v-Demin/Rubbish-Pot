[System.Serializable]
public class DestroyReaction : AbstractReaction
{
    public override void Execute(IReactionPart target)
    {
        target.Destroy();
    }
}
