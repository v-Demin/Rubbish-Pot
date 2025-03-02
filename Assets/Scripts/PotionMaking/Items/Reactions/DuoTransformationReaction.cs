using UnityEngine;

[System.Serializable]
public class DuoTransformationReaction : DuoReaction
{
    [Range(0f, 5f)] [SerializeField] private float _reactionFactor = 1f;
    [SerializeReference] private ProduceReaction _produceReaction = new ();
    [SerializeReference] private VolumeMultiplyReaction _volumeReaction = new ();
    [SerializeReference] private SoloReaction _destroyReaction = new DestroyReaction();
    
    public override void Execute(IReactionPart target, IReactionPart other)
    {
        var v3 = Mathf.Min(target.Volume * _reactionFactor, other.Volume);
        var v1 = target.Volume - (v3 / _reactionFactor);
        var v2 = other.Volume - v3;

        _volumeReaction.ManuallySet(target, v1);
        _volumeReaction.ManuallySet(other, v2);

        if (v3 >= 0.05f)
        {
            _produceReaction.ExecuteManually(target, v3);
        }
        
        if (v1 <= 0.05f)
        {
            _destroyReaction.Execute(target);
        }

        if (v2 <= 0.05f)
        {
            _destroyReaction.Execute(other);
        }
    }
}
