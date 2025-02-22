public interface ITemperatureHandler : ISubscriber
{
    public void HandleTemperatureChanged(float newTemperature);
}
