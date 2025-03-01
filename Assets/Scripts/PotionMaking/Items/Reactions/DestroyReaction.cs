[System.Serializable]
public class DestroyReaction : SoloReaction
{
    public override void Execute(IReactionPart target)
    {
        target.Destroy();
    }
}
