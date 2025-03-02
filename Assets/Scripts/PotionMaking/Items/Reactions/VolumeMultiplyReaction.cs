using UnityEngine;

[System.Serializable]
public class VolumeMultiplyReaction : SoloReaction
{
    //[Todo]: переделать на set, add и multiply подповедения
    
    [SerializeField] private float _volumeMultiplier = 1f;
    
    public override void Execute(IReactionPart target)
    {
        ManuallySet(target, target.Volume * _volumeMultiplier);
    }
    
    public void ManuallySet(IReactionPart target, float scale)
    {
        target.Volume = scale;
    }
}
