public interface ISpawnHandler : ISubscriber
{
    public void HandleSpawn(ReactionComponent target);
}