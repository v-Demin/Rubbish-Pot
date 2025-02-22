using UnityEngine;

[System.Serializable]
public class ReactionComponent
{
    private EventBus _configBus;
    
    [field:SerializeField] public ReactionConfig Config { get; private set; }

    public void Init()
    {
        _configBus = Config.EventBus;
        _configBus.RaiseEvent<ReactionConfig.ISpawnHandler>(h => h.HandleSpawn(this));
    }
    
    public void Collide(ReactionComponent other)
    {
        _configBus.RaiseEvent<ReactionConfig.ICollisionHandler>(h => h.HandleCollision(this, other));
    }
}
