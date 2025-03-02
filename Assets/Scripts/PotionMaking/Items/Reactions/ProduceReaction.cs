using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProduceReaction : SoloReaction
{
    [SerializeField] private List<TransformationInfo> _infos;
    [SerializeReference] private VolumeMultiplyReaction _volumeReaction = new ();

    public override void Execute(IReactionPart target)
    {
        foreach (var info in _infos)
        {
            ExecuteManually(target, info.Item, info.Volume, info.NumberOfItems);
        }
    }
    
    public void ExecuteManually(IReactionPart target, float totalVolume)
    {
        foreach (var info in _infos)
        {
            ExecuteManually(target, info.Item, totalVolume / _infos.Count, info.NumberOfItems);
        }
    }

    public void ExecuteManually(IReactionPart target, PotItem prefab, float volume, int numberOfItems)
    {
        for (var i = 0; i < numberOfItems; i++)
        {
            var item = Object.Instantiate(prefab);
            
            item.Init();
            _volumeReaction.ManuallySet(item.ReactionPart, volume);
            
            item.transform.position = GetPosition(target);
            item.transform.rotation = GetRotation();
        }
    }

    private Vector3 GetScaleVector(float scale)
    {
        return Vector3.one * scale;
    }

    private Vector3 GetPosition(IReactionPart target)
    {
        return target.Position + (Vector3)Random.insideUnitCircle * target.Volume;
    }
    
    private Quaternion GetRotation()
    {
        var z = Random.Range(0f, 360f);
        return Quaternion.Euler(new Vector3(0f, 0f, z));
    }
    
    [System.Serializable]
    private class TransformationInfo
    {
        public PotItem Item;
        public int NumberOfItems = 1;
        public float Volume = 1f;
    }
}
