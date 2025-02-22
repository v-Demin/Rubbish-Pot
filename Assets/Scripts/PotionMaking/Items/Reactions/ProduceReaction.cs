using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProduceReaction : AbstractReaction
{
    [SerializeField] private List<TransformationInfo> _infos;
    
    public override void Execute(IReactionPart target)
    {
        foreach (var info in _infos)
        {
            for (var i = 0; i < info.NumberOfItems; i++)
            {
                var item = GameObject.Instantiate(info.Item);
                item.transform.localScale = Vector3.one * info.Scale;
                item.transform.position = target.Position + ((Vector3)Random.insideUnitCircle * 0.1f);
            }
        }
    }
    
    [System.Serializable]
    private class TransformationInfo
    {
        public PotItem Item;
        public int NumberOfItems;
        public float Scale = 1f;
    }
}
