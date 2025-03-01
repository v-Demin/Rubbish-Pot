public interface IDuoConditionReachedHandler : ISubscriber
{
    public void HandleConditionReached(IReactionPart target, IReactionPart other);
}
