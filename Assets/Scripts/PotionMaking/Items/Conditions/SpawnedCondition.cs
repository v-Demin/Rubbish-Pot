public class SpawnedCondition : AbstractCondition, ISpawnHandler
{
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;

    public void HandleSpawn(ReactionComponent target)
    {
        ConditionReached(target);
    }
}
