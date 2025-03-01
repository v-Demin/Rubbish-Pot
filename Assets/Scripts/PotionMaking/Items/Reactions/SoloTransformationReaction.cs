using UnityEngine;

[System.Serializable]
public class SoloTransformationReaction : SoloReaction
{
    [SerializeReference] private SoloReaction _produceReaction = new ProduceReaction();
    [SerializeReference] private SoloReaction _destroyReaction = new DestroyReaction();

    public override void Execute(IReactionPart target)
    {
        _produceReaction.Execute(target);
        _destroyReaction.Execute(target);
    }
}
