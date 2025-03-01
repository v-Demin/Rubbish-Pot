using UnityEngine;

[System.Serializable]
public class SoloTransformationReaction : AbstractReaction
{
    [SerializeReference] private AbstractReaction _produceReaction = new ProduceReaction();
    [SerializeReference] private AbstractReaction _destroyReaction = new DestroyReaction();

    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;

    public override void Execute(IReactionPart target)
    {
        _produceReaction.Execute(target);
        _destroyReaction.Execute(target);
    }
}
