using System;
using UnityEngine;

[System.Serializable]
public class ReactionComponent : IReactionPart, ITemperatureHandler
{
    private EventBus _configBus;
    private Action _destroyCallback;
    
    [field:SerializeField] public ReactionConfig Config { get; private set; }

    public void Init(Action destroyCallback)
    {
        _configBus = Config.EventBus;
        
        _destroyCallback = destroyCallback;
        
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

    public void Destroy()
    {
        _destroyCallback?.Invoke();
    }
}

public interface IReactionPart
{
    public void Init(Action destroyCallback);
    public void Destroy();
}