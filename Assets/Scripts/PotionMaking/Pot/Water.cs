using UnityEngine;

public class Water : MonoBehaviour, IEventBusKeeper
{
    public const float MIN_TEMPERATURE = 5f;
    public const float MAX_TEMPERATURE = 100f;

    public EventBus EventBus { get; } = new EventBus();

    private float _temperature = MIN_TEMPERATURE;
    public float Temperature => _temperature;

    public void OnSliderUpdated(float sliderValue)
    {
        _temperature = Mathf.Lerp(MIN_TEMPERATURE, MAX_TEMPERATURE, sliderValue);
        EventBus.RaiseEvent<ITemperatureHandler>(h => h.HandleTemperatureChanged(_temperature));
    }
}
