using UnityEngine;

[System.Serializable]
public class VolumeMultiplyReaction : SoloReaction, ISoloConditionReachedHandler
{
    [SerializeField] private float _volumeMultiplier = 1f;
    
    public override void Execute(IReactionPart target)
    {
        ManualExecute(target, _volumeMultiplier);
    }
    
    public void ManualExecute(IReactionPart target, float multiplier)
    {
        target.Volume *= multiplier;
    }
}
