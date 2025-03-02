using System;
using UnityEngine;

[System.Serializable]
public class ReactionComponent : IReactionPart, ITemperatureHandler
{
    private EventBus _configBus;
    private EventBus _waterBus;
    private Action _destroyCallback;
    private PotItem _owner;
    public Vector3 Position => _owner.transform.position;

    public float Volume
    {
        get => _owner.Volume;
        set => _owner.Volume = value;
    }

    [field:SerializeField] public ReactionConfig Config { get; private set; }

    public void Init(PotItem owner, Action destroyCallback)
    {
        _configBus = Config.EventBus;
        _owner = owner;
        _destroyCallback = destroyCallback;
        
        _configBus.RaiseEvent<ISpawnHandler>(h => h.HandleSpawn(this));
    }
    
    public void Collide(ReactionComponent other)
    {
        _configBus?.RaiseEvent<ICollisionHandler>(h => h.HandleCollision(this, other));
    }
    
    public void Collide(Spoon other)
    {
        _configBus?.RaiseEvent<IMixingHandler>(h => h.HandleBeingMixed(this));
    }
    
    public void DipIntoWater(Water water)
    {
        _waterBus = water.EventBus;
        _waterBus.Subscribe(this);
        water.NotifyDipIntoWater(this);
        HandleTemperatureChanged(water.Temperature);
    }

    public void HandleTemperatureChanged(float newTemperature)
    {
        _configBus?.RaiseEvent<ITemperatureReactionHandler>(h => h.HandleTemperatureChanged(newTemperature, this));
    }

    public void Destroy()
    {
        _waterBus?.Unsubscribe(this);
        _destroyCallback?.Invoke();
    }
}

public interface IReactionPart
{

    public ReactionConfig Config { get;}
    public Vector3 Position { get; }
    public float Volume { get; set; }

    public void Destroy();
}