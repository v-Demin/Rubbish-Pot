public interface ISpawnHandler : ISubscriber
{
    public void HandleSpawn(IReactionPart target);
}