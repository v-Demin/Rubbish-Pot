using UnityEngine;

[System.Serializable]
public class ReactionComponent : ITemperatureHandler
{
    private EventBus _configBus;
    
    [field:SerializeField] public ReactionConfig Config { get; private set; }

    public void Init()
    {
        _configBus = Config.EventBus;
        _configBus.RaiseEvent<ISpawnHandler>(h => h.HandleSpawn(this));
    }
    
    public void Collide(ReactionComponent other)
    {
        _configBus.RaiseEvent<ICollisionHandler>(h => h.HandleCollision(this, other));
    }
    
    public void DipIntoWater(Water water)
    {
        water.EventBus.Subscribe(this);
        HandleTemperatureChanged(water.Temperature);
    }

    public void HandleTemperatureChanged(float newTemperature)
    {
        _configBus.RaiseEvent<ITemperatureReactionHandler>(h => h.HandleTemperatureChanged(newTemperature, this));
    }
}
