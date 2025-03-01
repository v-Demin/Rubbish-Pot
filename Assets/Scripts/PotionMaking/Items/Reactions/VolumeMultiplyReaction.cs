using UnityEngine;

[System.Serializable]
public class VolumeMultiplyReaction : AbstractReaction
{
    [SerializeField] private float _volumeMultiplier = 1f;
    
    public override IConnectinable.ConnectionType Connection => IConnectinable.ConnectionType.Solo;
    
    public override void Execute(IReactionPart target)
    {
        ManualExecute(target, _volumeMultiplier);
    }
    
    public void ManualExecute(IReactionPart target, float multiplier)
    {
        target.Volume *= multiplier;
    }
}
