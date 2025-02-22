public interface ITemperatureReactionHandler : ISubscriber
{
    public void HandleTemperatureChanged(float newTemperature, ReactionComponent component);
}
