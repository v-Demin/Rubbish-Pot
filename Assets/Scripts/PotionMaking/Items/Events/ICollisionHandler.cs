public interface ICollisionHandler : ISubscriber
{
    public void HandleCollision(ReactionComponent target, ReactionComponent other);
}