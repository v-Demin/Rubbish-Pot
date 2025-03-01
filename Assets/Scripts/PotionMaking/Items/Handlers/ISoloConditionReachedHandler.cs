public interface ISoloConditionReachedHandler : IDuoConditionReachedHandler
{
    public void HandleConditionReached(IReactionPart target);
}
