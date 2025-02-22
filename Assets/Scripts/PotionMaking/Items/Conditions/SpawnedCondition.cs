    public class SpawnedCondition : AbstractCondition, ISpawnHandler
    {
        public void HandleSpawn(ReactionComponent target)
        {
            ConditionReached(target);
        }
    }
