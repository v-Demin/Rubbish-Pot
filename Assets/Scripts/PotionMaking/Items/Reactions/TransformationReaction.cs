using UnityEngine;

[System.Serializable]
public class TransformationReaction : AbstractReaction
{
    [SerializeReference] private AbstractReaction _produceReaction = new ProduceReaction();
    [SerializeReference] private AbstractReaction _destroyReaction = new DestroyReaction();
    
    public override void Execute(IReactionPart target)
    {
        _produceReaction.Execute(target);
        _destroyReaction.Execute(target);
    }
}
