using UnityEngine;

public class TemperatureCondition : AbstractCondition, ITemperatureReactionHandler
{
    [Range(Water.MIN_TEMPERATURE, Water.MAX_TEMPERATURE)] [SerializeField] private float _lowerTemperature = Water.MIN_TEMPERATURE;
    [Range(Water.MIN_TEMPERATURE, Water.MAX_TEMPERATURE)] [SerializeField] private float _higherTemperature = Water.MAX_TEMPERATURE;
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;

    public void HandleTemperatureChanged(float newTemperature, IReactionPart target)
    {
        if (!(newTemperature >= _lowerTemperature) || !(newTemperature <= _higherTemperature)) return;
        
        EventBus.RaiseEvent<IAmbivalentConditionReachedHandler>(h => h.HandleConditionReached());
        EventBus.RaiseEvent<ISoloConditionReachedHandler>(h => h.HandleConditionReached(target));
    }
}
